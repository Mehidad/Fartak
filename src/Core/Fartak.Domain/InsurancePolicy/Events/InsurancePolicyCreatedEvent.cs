using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.InsurancePolicy.Events
{
    // ثبت بیمه‌نامهٔ جدید برای پرونده
    public sealed record InsurancePolicyCreatedEvent(
        long insurancePolicyId,
        long caseId,
        string companyName,
        string policyNo,
        DateTime? coverageStart,
        DateTime? coverageEnd,
        long? premiumRial,
        long statusId,
        string? meta) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
