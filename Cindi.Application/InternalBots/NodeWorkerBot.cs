using Cindi.Application.Entities.Queries;
using Cindi.Application.InternalBots.Clients;
using Cindi.Application.InternalBots.InternalSteps;
using Cindi.Domain.Entities.BotKeys;
using Cindi.Domain.Entities.Steps;
using Cindi.Domain.Utilities;
using Cindi.DotNetCore.BotExtensions;
using Cindi.DotNetCore.BotExtensions.Requests;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Cindi.Application.InternalBots
{
    public class NodeWorkerBot : WorkerBotHandler<WorkerBotHandlerOptions>
    {
        IMediator _mediator;
        private readonly IHttpClientFactory _clientFactory;

        public NodeWorkerBot(
            WorkerBotHandlerOptions options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IMediator mediatr,
            IHttpClientFactory clientFactory
            ) : base(options, logger, encoder)
        {
            _mediator = mediatr;
            _clientFactory = clientFactory;
        }

        public async override Task<UpdateStepRequest> HandleStep(Step step)
        {
            var updateRequest = new UpdateStepRequest()
            {
                Id = step.Id
            };

            switch (step.StepTemplateId)
            {
               /* case "_GenerateSystemReport:0":
                    var totalStepCount = (await _mediator.Send(new GetEntitiesQuery<Step>
                    {
                        Expression = e => e.P
                    })).Count;

                    var totalUnassignedStepCount = (await _mediator.Send(new GetEntitiesQuery<Step>
                    {
                        Page = 0,
                        Size = 0,
                        Expression = e => e.Status == StepStatuses.Unassigned
                    })).Count;

                    var totalActiveBotCount = (await _mediator.Send(new GetEntitiesQuery<BotKey>
                    {
                        Page = 0,
                        Size = 0,
                        Expression = e => e.IsDisabled == false
                    })).Count;

                    updateRequest.Outputs = new Dictionary<string, object>()
                    {
                        {  "report", "Total Steps: " + totalStepCount + ", Total Unassigned Steps:" + totalUnassignedStepCount + ", Total Active Bots: " + totalActiveBotCount },
                        { "slack_report", JsonConvert.SerializeObject(new[]
                            {
                               new
                               {
                                   type = "section",
                                   fields = new[] {
                                       new {
                                   text = "Total Unassigned Steps: " + totalUnassignedStepCount,
                                   type = "mrkdwn"
                                       }
                               }
                               },
                               new
                               {
                                   type = "section",
                                   fields = new[] {
                                       new {
                                   text = "Total Steps: " + totalStepCount,
                                   type = "mrkdwn"
                                       }
                               }
                               },
                                                              new
                               {
                                   type = "section",
                                   fields = new[] {
                                       new {
                                   text = "Total Active Bots: " + totalActiveBotCount,
                                   type = "mrkdwn"
                                       }
                               }

                            }
                        })
                        },
                        { "markdown", "" }

                    };

                    updateRequest.Status = StepStatuses.Successful;
                    updateRequest.StatusCode = 0;
                    return updateRequest;
                case "_SendSlackMessage:0":
                    var client = new SlackClient(_clientFactory);
                    await client.PostMessage((string)DynamicDataUtility.GetData(step.Inputs, "webhook_url").Value, new
                    {
                        username = (string)DynamicDataUtility.GetData(step.Inputs, "from").Value,
                        icon_emoji = step.Inputs.ContainsKey("icon_emoji") ? (string)DynamicDataUtility.GetData(step.Inputs, "icon_emoji").Value : null,
                        icon_url = step.Inputs.ContainsKey("icon_url") ? (string)DynamicDataUtility.GetData(step.Inputs, "icon_url").Value : null,
                        channel = step.Inputs.ContainsKey("channel") ? (string)DynamicDataUtility.GetData(step.Inputs, "channel").Value : null,
                        blocks = step.Inputs.ContainsKey("blocks") ? JsonConvert.DeserializeObject((string)DynamicDataUtility.GetData(step.Inputs, "blocks").Value) : null,
                        text = step.Inputs.ContainsKey("text") ? (string)DynamicDataUtility.GetData(step.Inputs, "text").Value : null
                    });
                    updateRequest.Status = StepStatuses.Successful;
                    return updateRequest;*/
            }

            updateRequest.Status = StepStatuses.Error;
            updateRequest.Log = "Bot does not have a catch for " + step.StepTemplateId;
            return updateRequest;
        }
    }
}
