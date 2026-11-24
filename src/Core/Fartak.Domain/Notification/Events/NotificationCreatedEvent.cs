using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Notification.Events
{
    // ایجاد اعلان جدید برای کاربر
    public sealed record NotificationCreatedEvent(
        long notificationId,
        long recipientId,
        long typeId,
        string title,
        string? body,
        long? caseId,
        string? relatedEntityType,
        long? relatedEntityId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
