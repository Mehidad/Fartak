using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Payment.Events
{
    // گذار وضعیت پرداخت (در انتظار/پرداخت‌شده/لغوشده/مردود و ...)
    public sealed record PaymentStatusChangedEvent(
        long paymentId,
        long caseId,
        long oldStatusId,
        long newStatusId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
