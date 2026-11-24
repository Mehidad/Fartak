using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Notification.Events
{
    // خوانده‌شدن اعلان توسط گیرنده
    public sealed record NotificationReadEvent(
        long notificationId,
        long recipientId,
        DateTime readAt) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
