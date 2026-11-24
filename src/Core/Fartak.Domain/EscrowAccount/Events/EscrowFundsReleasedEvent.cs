using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.EscrowAccount.Events
{
    // آزادسازی وجه به دریافت‌کننده پس از کسر کارمزد پلتفرم
    public sealed record EscrowFundsReleasedEvent(
        long escrowAccountId,
        long caseId,
        long farmerId,
        long? expertId,
        long payoutAmountRial,
        long commissionAmountRial,
        DateTime releasedAt) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
