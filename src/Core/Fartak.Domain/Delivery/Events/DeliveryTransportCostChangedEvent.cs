using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Delivery.Events
{
    // تغییر هزینهٔ حمل تحویل
    public sealed record DeliveryTransportCostChangedEvent(
        long deliveryId,
        long caseId,
        long oldTransportCostRial,
        long newTransportCostRial) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
