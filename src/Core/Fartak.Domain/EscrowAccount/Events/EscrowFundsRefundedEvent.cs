using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.EscrowAccount.Events
{
    // برگرداندن کامل وجه به کشاورز (انصراف/فسخ)
    public sealed record EscrowFundsRefundedEvent(
        long escrowAccountId,
        long caseId,
        long farmerId,
        long heldAmountRial,
        DateTime refundedAt) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
