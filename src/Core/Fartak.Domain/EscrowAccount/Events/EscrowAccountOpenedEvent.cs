using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.EscrowAccount.Events
{
    // افتتاح حساب امانی با Snapshot کارمزد و سهم پرداختی
    public sealed record EscrowAccountOpenedEvent(
        long escrowAccountId,
        long caseId,
        long? paymentId,
        long farmerId,
        long? expertId,
        long heldAmountRial,
        decimal commissionRate,
        long commissionAmountRial,
        long payoutAmountRial,
        long statusId,
        DateTime openedAt) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
