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

namespace Cindi.Presentation
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            MongoClient = new MongoClient(Configuration.GetValue<string>("ConnectionStrings:CindiDB"));
        }

        public IConfiguration Configuration { get; }
        public IMongoClient MongoClient { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMediatR(typeof(CreateStepTemplateCommandHandler).GetTypeInfo().Assembly);
            services.AddMediatR(typeof(GetStepTemplatesQueryHandler).GetTypeInfo().Assembly);
            services.AddMediatR(typeof(CreateStepCommandHandler).GetTypeInfo().Assembly);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            //Add step template
            services.AddTransient<IStepTemplatesRepository, StepTemplatesRepository>(s => new StepTemplatesRepository(MongoClient));
            services.AddTransient<IStepsRepository, StepsRepository>(s => new StepsRepository(MongoClient));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            BaseRepository.RegisterClassMaps();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseMvc();
        }
    }
}
