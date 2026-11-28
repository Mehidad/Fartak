using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.InsurancePolicy.Events
{
    // تغییر وضعیت بیمه‌نامه (lookup: insurance_status)
    public sealed record InsurancePolicyStatusChangedEvent(
        long insurancePolicyId,
        long caseId,
        long oldStatusId,
        long newStatusId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
