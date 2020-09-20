using Cindi.Application.InternalBots.InternalSteps;
using Cindi.Domain.Entities.StepTemplates;
using Cindi.DotNetCore.BotExtensions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;

namespace Cindi.Application.InternalBots
{
    public class InternalBotManager
    {
        public List<WorkerBotHandler<WorkerBotHandlerOptions>> Bots = new List<WorkerBotHandler<WorkerBotHandlerOptions>>();
        private ILoggerFactory _logger;
        private UrlEncoder _encoder;
        private IMediator _mediatr;
        private IHttpClientFactory _httpClientFactory;
        private IOptionsMonitor<WorkerBotHandlerOptions> _options;
        private IConfiguration _configuration;


        public InternalBotManager(
            ILoggerFactory logger,
            UrlEncoder encoder,
            IMediator mediatr,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _logger = logger;
            _encoder = encoder;
            _mediatr = mediatr;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public void AddAdditionalBot()
        {
            var bot = new NodeWorkerBot(
                new WorkerBotHandlerOptions()
                {
                    NodeURL = _configuration.GetValue<string>("Cluster:NodeUrls").Split(",")[0],
                    AutoStart = true,
                    AutoRegister = true,
                    SleepTime = 0,
                        StepTemplateLibrary = new List<StepTemplate> {
                            InternalStepLibrary.GenerateSystemReport,
                            InternalStepLibrary.SendSlackMessage
                        }
                    },
                   _logger,
                   _encoder,
                   _mediatr,
                   _httpClientFactory
                );
            Bots.Add(bot);
        }
    }
}
