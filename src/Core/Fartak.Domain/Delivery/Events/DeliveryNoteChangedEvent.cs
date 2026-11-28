using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Delivery.Events
{
    // تغییر یادداشت تحویل
    public sealed record DeliveryNoteChangedEvent(
        long deliveryId,
        long caseId,
        string? oldNote,
        string? newNote) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
