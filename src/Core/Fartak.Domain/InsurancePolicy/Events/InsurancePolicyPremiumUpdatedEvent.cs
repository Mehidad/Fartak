using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.InsurancePolicy.Events
{
    // به‌روزرسانی حق بیمه
    public sealed record InsurancePolicyPremiumUpdatedEvent(
        long insurancePolicyId,
        long caseId,
        long? oldPremiumRial,
        long? newPremiumRial) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
