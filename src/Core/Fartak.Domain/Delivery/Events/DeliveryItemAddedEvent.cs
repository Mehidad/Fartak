using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Delivery.Events
{
    // افزودن ردیف به تحویل
    public sealed record DeliveryItemAddedEvent(
        long deliveryId,
        long deliveryItemId,
        long? planLineId,
        long? batchLineId,
        decimal quantity,
        string? note) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
