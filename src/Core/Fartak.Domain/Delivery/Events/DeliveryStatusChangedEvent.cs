using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Delivery.Events
{
    // تغییر وضعیت تحویل (lookup: delivery_status)
    public sealed record DeliveryStatusChangedEvent(
        long deliveryId,
        long caseId,
        long oldStatusId,
        long newStatusId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
