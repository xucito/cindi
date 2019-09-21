using Cindi.Application.GlobalValues.Commands.CreateGlobalValue;
using Cindi.Application.Workflows.Commands;
using Cindi.Application.Steps.Commands.CreateStep;
using Cindi.Application.Users.Commands.CreateUserCommand;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Cindi.Application.Workflows.Commands.CreateWorkflow;

namespace Cindi.Application.Pipelines
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        public ILogger<PipelineLogger> Logger;
        private const int maxNumberOfResults = 10;
        private const string secretOmmited = "******SECURITY OMISSION******";
        private const string lengthOmmited = "******LENGTH OMISSION******";
        public LoggingBehavior(ILogger<PipelineLogger> logger)
        {
            Logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            Stopwatch watch = null;
            Guid processId = Guid.NewGuid();
            TResponse response;
            if (Logger.IsEnabled(LogLevel.Debug))
            {
                watch = Stopwatch.StartNew();
                Logger.LogDebug($"Handling {typeof(TRequest).Name}" + " Request Id: " + processId + Environment.NewLine + (ShouldPrintOutput(typeof(TRequest)) ? SerializeAndHideSecrets(request) : secretOmmited));
            }

            try
            {
                response = await next();
            }
            catch(Exception e)
            {
                Logger.LogError("Encounter exception while processing request " +  $"{typeof(TRequest).Name}:" + processId + Environment.NewLine + e.StackTrace);
                throw e;
            }

            if (Logger.IsEnabled(LogLevel.Debug))
            {
                watch.Stop();
                Logger.LogDebug($"Handled {typeof(TRequest).Name}" + " Request Id: " + processId + " took " + watch.ElapsedMilliseconds + "ms " + $"returning {typeof(TResponse).Name}" + Environment.NewLine + (ShouldPrintOutput(typeof(TRequest)) ? SerializeAndHideSecrets(response) : secretOmmited));
            }
            return response;
        }

        public bool ShouldPrintOutput(Type request)
        {
            if(request == typeof(CreateStepCommand) ||
                request == typeof(CreateWorkflowCommand) ||
                request == typeof(CreateUserCommand) || 
                request == typeof(CreateGlobalValueCommand))
            {
                return false;
            }
            return true;
        }

        public string SerializeAndHideSecrets(object item)
        {
            try
            {
                var token = JObject.FromObject(item);

                if (token.Type == JTokenType.Object)
                {
                    foreach (JProperty prop in token.Children<JProperty>().ToList())
                    {
                        if (prop.Name.ToLower() == "password")
                        {
                            prop.Value = secretOmmited;
                        }
                    }
                }
                else if (token.Type == JTokenType.Array)
                {
                    foreach (JToken child in token.Children())
                    {
                        SerializeAndHideSecrets(child);
                    }
                }

                var finalString = token.ToString().Split('\n').ToList();
                if (finalString.Count() > maxNumberOfResults)
                {
                    finalString = finalString.Take(maxNumberOfResults).ToList();
                    finalString.Add(lengthOmmited);
                }
                return String.Join(Environment.NewLine, finalString);
            }
            catch(Exception e)
            {
                throw e;
            }
        }
    }

    public class PipelineLogger
    {

    }
}
