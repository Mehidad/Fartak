using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Delivery.Events
{
    // تأیید تحویل توسط کشاورز
    public sealed record DeliveryConfirmedEvent(
        long deliveryId,
        long caseId,
        DateTime deliveredAt,
        DateTime confirmedAt) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
