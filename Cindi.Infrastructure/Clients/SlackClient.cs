using Cindi.Application.Interfaces;
using Cindi.Domain.ValueObjects;
using Cindi.Infrastructure.NotificationService;
using Cindi.Infrastructure.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Cindi.Infrastructure.Clients
{
    public class SlackClient : INotificationClient
    {
        public SlackClientOptions _slackOptions { get; set; }
        NotificationClientOptions _options;
        public NotificationClientOptions Options
        {
            get
            {
                return _options;
            }
            set
            {
                _slackOptions = value.ClientOptions as SlackClientOptions;
                _options = value;
            }
        }

        public SlackClient()
        {
        }

        public void Send(Notification message)
        {
            using (WebClient webClient = new WebClient())
            {
                webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                byte[] request = System.Text.Encoding.UTF8.GetBytes("payload=" + JsonConvert.SerializeObject(message));
                byte[] response = webClient.UploadData(_slackOptions.WebHookUri, "POST", request);
            }
        }

        public static bool IsOptionsValid(NotificationClientOptions option)
        {
            var convertedObject = option.ClientOptions as SlackClientOptions;
            return convertedObject != null;
        }

        public sealed class SlackMessage
        {
            [JsonProperty("channel")]
            public string Channel { get; set; }

            [JsonProperty("username")]
            public string UserName { get; set; }

            [JsonProperty("text")]
            public string Text { get; set; }

            [JsonProperty("icon_emoji")]
            public string Icon
            {
                get { return ":computer:"; }
            }
        }
    }
}
