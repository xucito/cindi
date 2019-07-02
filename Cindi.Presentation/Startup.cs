using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MediatR;
using System.Reflection;
using Swashbuckle.AspNetCore.Swagger;
using Cindi.Application.StepTemplates.Commands;
using MongoDB.Driver;
using Cindi.Application.StepTemplates.Commands.CreateStepTemplate;
using Cindi.Application.StepTemplates.Queries.GetStepTemplates;
using MongoDB.Bson.Serialization;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.Persistence.StepTemplates;
using Cindi.Persistence.Steps;
using Cindi.Domain.Entities.Steps;
using Cindi.Application.Interfaces;
using Cindi.Persistence;
using Cindi.Application.Steps.Commands;
using Cindi.Application.Steps.Commands.CreateStep;
using Cindi.Application.Services.ClusterState;
using Cindi.Presentation.Transformers;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Cindi.Persistence.SequenceTemplates;
using Cindi.Persistence.Sequences;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Cindi.Application.Options;
using Cindi.Application.Services.ClusterMonitor;
using Microsoft.AspNetCore.Authentication;
using Cindi.Presentation.Authentication;
using Cindi.Persistence.Users;
using Swashbuckle.AspNetCore.Filters;
using Cindi.Application.Cluster.Commands.InitializeCluster;
using Cindi.Persistence.BotKeys;
using Cindi.Presentation.Middleware;
using Cindi.Domain.Exceptions.Utility;
using AutoMapper;
using Cindi.Persistence.GlobalValues;
using Cindi.Persistence.State;
using ConsensusCore.Node.Utility;
using SlugifyParameterTransformer = Cindi.Presentation.Transformers.SlugifyParameterTransformer;
using ConsensusCore.Node;
using ConsensusCore.Domain.Interfaces;
using Cindi.Application.Services;
using ConsensusCore.Node.Services;
using System.Threading;
using ConsensusCore.Node.Controllers;
using ConsensusCore.Domain.Services;
using Cindi.Domain.Entities.States;

namespace Cindi.Presentation
{
    public class Startup
    {

        public Task BootstrapThread;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            MongoClient = new MongoClient(Configuration.GetValue<string>("DBConnectionString"));
            EnableUI = Configuration.GetValue<bool>("EnableUI");
        }

        public IConfiguration Configuration { get; }
        public IMongoClient MongoClient { get; set; }
        public bool EnableUI { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddTransient<IDataRouter, CindiDataRouter>();
            services.AddConsensusCore<CindiClusterState, INodeStorageRepository>(s => new NodeStorageRepository(MongoClient));

            services.AddScoped<IMediator, Mediator>();
            services.AddMediatR(typeof(CreateStepTemplateCommandHandler).GetTypeInfo().Assembly);
            // services.AddMediatR(typeof(CreateStepCommandHandler).GetTypeInfo().Assembly);
            services.AddAutoMapper();

            services.AddMvc(options =>
            {
                options.Conventions.Add(new RouteTokenTransformerConvention(
                                new SlugifyParameterTransformer()));
            }
            ).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.Configure<CindiClusterOptions>(option => new CindiClusterOptions
            {
                DefaultSuspensionTime = Configuration.GetValue<double>("DefaultSuspensionTimeMs"),
                DbConnectionString = Configuration.GetValue<string>("DBConnectionString")
            });

            if (EnableUI)
            {
                services.AddSpaStaticFiles(configuration =>
                {
                    configuration.RootPath = "ClientApp/dist";
                });
            }

            var value = Configuration.GetSection("Cluster").GetValue<string>("Urls");
            // Options
            services.Configure<ClusterOptions>((action) =>
            {
                action.NodeUrls = Configuration.GetSection("Cluster").GetValue<string>("Urls").Split(",").ToList();
                action.MinimumNodes = Configuration.GetSection("Cluster").GetValue<int>("MinimumNodes");
                action.NumberOfShards = 1;
                action.DataTransferTimeoutMs = 30000;
            });

            services.Configure<NodeOptions>((action) => new NodeOptions()
            {
                EnableLeader = true
            });

            //Add step template
            services.AddTransient<IStepTemplatesRepository, StepTemplatesRepository>(s => new StepTemplatesRepository(MongoClient));
            services.AddTransient<IStepsRepository, StepsRepository>(s => new StepsRepository(MongoClient));
            services.AddTransient<ISequencesRepository, SequencesRepository>(s => new SequencesRepository(MongoClient));
            services.AddTransient<ISequenceTemplatesRepository, SequenceTemplatesRepository>(s => new SequenceTemplatesRepository(MongoClient));
            // services.AddTransient<IClusterRepository, ClusterRepository>(s => new ClusterRepository(MongoClient));
            services.AddTransient<IUsersRepository, UsersRepository>(s => new UsersRepository(MongoClient));
            services.AddTransient<IBotKeysRepository, BotKeysRepository>(s => new BotKeysRepository(MongoClient));
            services.AddSingleton<IClusterStateService, ClusterStateService>();
            services.AddSingleton<IGlobalValuesRepository, GlobalValuesRepository>(s => new GlobalValuesRepository(MongoClient));
            // services.AddSingleton<ClusterStateService>();
            services.AddSingleton<ClusterMonitorService>();




            //Authentication
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "smart";
                    options.DefaultChallengeScheme = "smart";
                })
                .AddPolicyScheme("smart", "SMART", options =>
                  {
                      options.ForwardDefaultSelector = context =>
                      {
                          var authHeader = context.Request.Headers["botKey"];
                          if (authHeader.FirstOrDefault() != null)
                          {
                              return "bot";
                          }
                          else if (context.Request.Method == "OPTIONS")
                          {
                              return "options";
                          }
                          else if (context.Request.Path == "/api/bot-keys" && context.Request.Method == "POST")
                          {
                              return "options";
                          }
                          else if (context.Request.Path.Value.Contains("/api/node"))
                          {
                              return "options";
                          }

                          return "basic";
                      };
                  })
                .AddScheme<AuthenticationSchemeOptions, OptionsAuthenticationHandler>("options", null)
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("basic", null)
                .AddScheme<AuthenticationSchemeOptions, BotAuthenticationHandler>("bot", null);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
                c.AddSecurityDefinition("Basic", new BasicAuthScheme { Type = "basic" });
                c.DocumentFilter<BasicAuthenticationFilter>();
            });

            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                                                                        .AllowAnyMethod()
                                                                         .AllowAnyHeader()));


            BaseRepository.RegisterClassMaps();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            IClusterStateService service,
            ClusterMonitorService monitor,
            ILogger<Startup> logger,
            IConsensusCoreNode<CindiClusterState, IBaseRepository> node,
            IMediator mediator,
            IServiceProvider serviceProvider)
        {
            BootstrapThread = new Task(() =>
            {
                while (node.NodeInfo.Status != ConsensusCore.Domain.Models.NodeStatus.Green)
                {
                    logger.LogInformation("Waiting for cluster to establish a quorum");
                    Thread.Sleep(1000);
                }

                if(node.GetState().Initialized)
                { }

                ClusterStateService.Initialized = node.GetState().Initialized;

                var key = Configuration.GetValue<string>("EncryptionKey");
                if (key != null)
                {
                    if (ClusterStateService.Initialized)
                    {
                        logger.LogWarning("Loading key in configuration file, this is not recommended for production.");
                        try
                        {
                            service.SetEncryptionKey(key);
                            logger.LogInformation("Successfully applied encryption key.");
                        }
                        catch (InvalidPrivateKeyException e)
                        {
                            logger.LogError("Failed to apply stored key. Key does not match registered encryption hash.");
                        }
                    }
                }


                if (!ClusterStateService.Initialized)
                {

                    if (key != null)
                    {
                        logger.LogWarning("Initializing new node with key in configuration file, this is not recommended for production.");
                        service.SetEncryptionKey(key);
                    }
                    else
                    {
                        logger.LogWarning("No default key detected, post key to /api/cluster/encryption-key.");
                    }

                    var setPassword = Configuration.GetValue<string>("DefaultPassword");




                    if (node.IsLeader)
                    {
                        // Thread.Sleep(5000);
                        var med = (IMediator)app.ApplicationServices.CreateScope().ServiceProvider.GetService(typeof(IMediator));
                        med.Send(new InitializeClusterCommand()
                        {
                            DefaultPassword = setPassword == null ? "PleaseChangeMe" : setPassword,
                            Name = Configuration.GetValue<string>("ClusterName")
                        }).GetAwaiter().GetResult();
                    }
                }
            });
            BootstrapThread.Start();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                //c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
            });

            app.UseCindiClusterPipeline();

            app.UseAuthentication();

            app.UseCors("AllowAll");

            app.UseMvc();

            if (EnableUI)
            {
                app.UseSpaStaticFiles();
                app.UseSpa(spa =>
                {
                    // To learn more about options for serving an Angular SPA from ASP.NET Core,
                    // see https://go.microsoft.com/fwlink/?linkid=864501


                    if (env.IsDevelopment())
                    {
                        spa.Options.SourcePath = "ClientApp";
                        spa.UseAngularCliServer(npmScript: "start");
                    }
                });
            }
        }
    }
}
