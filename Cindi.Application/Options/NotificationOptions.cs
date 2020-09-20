using Cindi.Application.Interfaces;
using Cindi.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Infrastructure.Options
{
    public class NotificationClientOptions
    {
        public NotificationLevel EnabledLevel { get; set; }
        public object ClientOptions { get; set; }
        public string Type { get; set; }
    }

    public class NotificationOptions
    {
        public Dictionary<string, INotificationClient> Clients { get; set; }
    }
}
