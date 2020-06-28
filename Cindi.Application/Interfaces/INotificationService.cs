using Cindi.Domain.ValueObjects;
using Cindi.Infrastructure.Options;

namespace Cindi.Application.Interfaces
{
    public interface INotificationService
    {
        NotificationOptions Options { get; set; }

        void Send(Notification notification);
    }
}