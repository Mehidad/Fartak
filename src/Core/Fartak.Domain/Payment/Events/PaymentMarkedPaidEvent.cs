using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Payment.Events
{
    // ثبت قطعی پرداخت — مبنا برای آزادسازی حساب امانی و اعلان به طرفین
    public sealed record PaymentMarkedPaidEvent(
        long paymentId,
        long caseId,
        long amountRial,
        string? referenceNo,
        DateTime paidAt) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
