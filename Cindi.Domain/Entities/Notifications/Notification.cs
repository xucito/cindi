using Cindi.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.ValueObjects
{
    public class Notification
    {
        public NotificationLevel Level { get;set; }
        public string Message { get; set; }
    }
}
