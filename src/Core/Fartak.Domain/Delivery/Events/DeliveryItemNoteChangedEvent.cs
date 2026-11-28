using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Delivery.Events
{
    // تغییر یادداشت ردیف تحویل
    public sealed record DeliveryItemNoteChangedEvent(
        long deliveryId,
        long deliveryItemId,
        string? oldNote,
        string? newNote) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
