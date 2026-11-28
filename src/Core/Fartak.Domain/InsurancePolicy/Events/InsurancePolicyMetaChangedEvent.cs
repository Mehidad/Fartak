using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.InsurancePolicy.Events
{
    // تغییر Meta بیمه‌نامه (jsonb آزاد)
    public sealed record InsurancePolicyMetaChangedEvent(
        long insurancePolicyId,
        long caseId,
        string? oldMeta,
        string? newMeta) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
