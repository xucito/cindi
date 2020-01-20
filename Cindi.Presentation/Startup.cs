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
using System.IO;
using Cindi.Application.Pipelines;
using Cindi.Persistence.Workflows;
using Cindi.Persistence.WorkflowTemplates;
using Cindi.Persistence.Metrics;
using Cindi.Persistence.MetricTicks;
using ConsensusCore.Node.Services.Raft;
using ConsensusCore.Node.Services.Data;
using ConsensusCore.Node.Services.Tasks;

namespace Cindi.Presentation
{
    public class Startup
    {

        public Task BootstrapThread;
        public IConfiguration Configuration { get; }
        public IMongoClient MongoClient { get; set; }
        public bool EnableUI { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            MongoClient = new MongoClient(Configuration.GetValue<string>("DBConnectionString"));
            EnableUI = Configuration.GetValue<bool>("EnableUI");
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDataRouter, CindiDataRouter>();
            services.AddConsensusCore<CindiClusterState, INodeStorageRepository, INodeStorageRepository, INodeStorageRepository>(
                s => new NodeStorageRepository(MongoClient),
                s => new NodeStorageRepository(MongoClient),
                s => new NodeStorageRepository(MongoClient),
                Configuration.GetSection("Node")
                , Configuration.GetSection("Cluster"));

            //services.AddScoped<IMediator, Mediator>();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
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
                DefaultSuspensionTimeMs = Configuration.GetValue<double>("DefaultSuspensionTimeMs"),
                DbConnectionString = Configuration.GetValue<string>("DBConnectionString")
            });

            if (EnableUI)
            {
                services.AddSpaStaticFiles(configuration =>
                {
                    configuration.RootPath = "ClientApp/dist";
                });
            }

            //Add step template
            services.AddTransient<IStepTemplatesRepository, StepTemplatesRepository>(s => new StepTemplatesRepository(MongoClient));
            services.AddTransient<IStepsRepository, StepsRepository>(s => new StepsRepository(MongoClient));
            services.AddTransient<IWorkflowsRepository, WorkflowsRepository>(s => new WorkflowsRepository(MongoClient));
            services.AddTransient<IWorkflowTemplatesRepository, WorkflowTemplatesRepository>(s => new WorkflowTemplatesRepository(MongoClient));
            // services.AddTransient<IClusterRepository, ClusterRepository>(s => new ClusterRepository(MongoClient));
            services.AddTransient<IUsersRepository, UsersRepository>(s => new UsersRepository(MongoClient));
            services.AddTransient<IBotKeysRepository, BotKeysRepository>(s => new BotKeysRepository(MongoClient));
            services.AddSingleton<IClusterStateService, ClusterStateService>();
            services.AddSingleton<IGlobalValuesRepository, GlobalValuesRepository>(s => new GlobalValuesRepository(MongoClient));
            services.AddSingleton<IMetricsRepository, MetricsRepository>(s => new MetricsRepository(MongoClient));
            services.AddSingleton<IMetricTicksRepository, MetricTicksRepository>(s => new MetricTicksRepository(MongoClient));
            // services.AddSingleton<ClusterStateService>();


            services.AddTransient<IDatabaseMetricsCollector, MongoDBMetricsCollector>(s => new MongoDBMetricsCollector(MongoClient));
            services.AddSingleton<ClusterMonitorService>();
            services.AddSingleton<MetricManagementService>();




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
            ILogger<Startup> logger,
            IRaftService raftService,
            IDataService dataService,
            ITaskService taskService,
            IStateMachine<CindiClusterState> stateMachine,
            NodeStateService node,
            IDatabaseMetricsCollector collector,
            ClusterMonitorService monitor,
            IMediator mediator,
            IServiceProvider serviceProvider,
            MetricManagementService metricManagementService
            )
        {
            BootstrapThread = new Task(() =>
            {
                while (node.Status != ConsensusCore.Domain.Models.NodeStatus.Yellow &&
                node.Status != ConsensusCore.Domain.Models.NodeStatus.Green
                )
                {
                    logger.LogInformation("Waiting for cluster to establish a quorum");
                    Thread.Sleep(1000);
                }
                ClusterStateService.Initialized = stateMachine.CurrentState.Initialized;
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


                    if (node.Role == ConsensusCore.Domain.Enums.NodeState.Leader)
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

                if (node.Role == ConsensusCore.Domain.Enums.NodeState.Leader)
                {
                    metricManagementService.InitializeMetricStore();
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
                OverrideUISettings();
                app.UseSpaStaticFiles();
                app.UseSpa(spa =>
                {
                    // To learn more about options for serving an Angular SPA from ASP.NET Core,
                    // see https://go.microsoft.com/fwlink/?linkid=864501

                    spa.Options.SourcePath = "ClientApp";

                    if (env.IsDevelopment())
                    {
                        spa.UseAngularCliServer(npmScript: "start");
                    }
                });
            }
        }

        public void OverrideUISettings()
        {
            if (File.Exists("ClientApp/dist/env.js"))
            {
                string text = "";
                using (StreamReader sr = new StreamReader("ClientApp/dist/env.js"))
                {
                    int i = 0;
                    do
                    {
                        i++;
                        string line = sr.ReadLine();
                        if (line != null && line != "" && line.Contains("window.__env"))
                        {
                            var splitLine = line.Split("=");
                            var variableName = splitLine.First().Split(".").Last().Trim();
                            if (Configuration.GetSection("Client").GetValue<string>(variableName) != null)
                            {
                                text += splitLine.First() + "= \"" + Configuration.GetSection("Client").GetValue<string>(variableName) + "\";" + Environment.NewLine;
                            }
                            else
                            {
                                text += line + Environment.NewLine; ;
                            }
                        }
                        else
                        {
                            text += line + Environment.NewLine; ;
                        }
                    } while (sr.EndOfStream == false);
                }
                File.WriteAllText("./ClientApp/dist/env.js", text);
            }
        }
    }
}
