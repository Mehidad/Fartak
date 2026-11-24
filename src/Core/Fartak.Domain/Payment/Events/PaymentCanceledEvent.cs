using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Payment.Events
{
    // لغو پرداختِ ثبت‌نشده
    public sealed record PaymentCanceledEvent(
        long paymentId,
        long caseId,
        long amountRial,
        string? reason,
        DateTime canceledAt) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
