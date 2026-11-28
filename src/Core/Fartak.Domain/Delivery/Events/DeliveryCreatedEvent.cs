using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Delivery.Events
{
    // ثبت تحویل جدید برای پرونده
    public sealed record DeliveryCreatedEvent(
        long deliveryId,
        long caseId,
        long? batchId,
        long statusId,
        string? note) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
