using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.InsurancePolicy.Events
{
    // به‌روزرسانی بازهٔ پوشش بیمه‌نامه
    public sealed record InsurancePolicyCoverageUpdatedEvent(
        long insurancePolicyId,
        long caseId,
        DateTime? oldCoverageStart,
        DateTime? oldCoverageEnd,
        DateTime? newCoverageStart,
        DateTime? newCoverageEnd) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
