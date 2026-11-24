using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Contract.Events
{
    // پاک‌شدن ضمانت‌نامه بانکی قرارداد
    public sealed record ContractBankGuaranteeClearedEvent(
        long contractId,
        long caseId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
