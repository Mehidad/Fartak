using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Contract.Events
{
    // ثبت ضمانت‌نامه بانکی روی قرارداد
    public sealed record ContractBankGuaranteeSetEvent(
        long contractId,
        long caseId,
        string bankName,
        string guaranteeNumber,
        long amountRial,
        DateOnly validUntil) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
