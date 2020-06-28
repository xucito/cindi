using Cindi.Domain.ValueObjects;
using Cindi.Infrastructure.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Application.Interfaces
{
    public interface INotificationClient
    {
        NotificationClientOptions Options { get; set; }
        void Send(Notification notification);
    }
}
