using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Delivery.Events
{
    // ثبت زمان تحویل + تحویل‌گیرنده
    public sealed record DeliveryMarkedDeliveredEvent(
        long deliveryId,
        long caseId,
        DateTime deliveredAt,
        long? receivedBy) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
