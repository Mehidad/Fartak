using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Payment.Events
{
    // ثبت پرداخت جدید در چارچوب پرونده
    public sealed record PaymentCreatedEvent(
        long paymentId,
        long caseId,
        long? contractId,
        long? payerId,
        long? payeeId,
        long amountRial,
        long kindId,
        long? methodId,
        long statusId,
        string? referenceNo,
        string? note) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
