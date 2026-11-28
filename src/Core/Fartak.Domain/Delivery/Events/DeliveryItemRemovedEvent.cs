using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Delivery.Events
{
    // حذف ردیف از تحویل
    public sealed record DeliveryItemRemovedEvent(
        long deliveryId,
        long deliveryItemId,
        long? planLineId,
        long? batchLineId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
