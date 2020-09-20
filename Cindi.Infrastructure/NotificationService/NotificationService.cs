using Cindi.Application.Interfaces;
using Cindi.Domain.ValueObjects;
using Cindi.Infrastructure.Clients;
using Cindi.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Infrastructure.NotificationService
{
    public class NotificationService : INotificationService
    {
        ILogger<NotificationService> _logger;
        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;
        }

        public void Send(Notification notification)
        {
            foreach(var option in _options.Clients)
            {
                try
                {
                    _logger.LogDebug("Sending log " + notification.Message + " with client " + option.Key);
                    option.Value.Send(notification);
                }
                catch(Exception e)
                {
                    _logger.LogError("Failed to send notification on client " + option.Key + " with error " + e.Message + Environment.NewLine + e.StackTrace);
                }
            }
        }

        public NotificationOptions _options { get; set; }
        public NotificationOptions Options
        {
            get { return _options; }
            set
            {
                ValidateOptions(value);
                _options = value;
            }
        }

        public static void ValidateOptions(NotificationOptions options)
        {
            Dictionary<string, INotificationClient> clients = options.Clients;
            foreach (var client in clients)
            {
                var isOptionsValid = true;
                switch (client.Value.Options.Type)
                {
                    case "slack":
                        isOptionsValid = SlackClient.IsOptionsValid(client.Value.Options);
                        break;
                    default:
                        throw new Exception("Type " + client.Value.Options.Type + " is not supported.");
                }
                if (!isOptionsValid)
                {
                    throw new Exception("Failed to validate client options for " + client.Value.Options.Type + ".");
                }
            }
        }
    }
}
