using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Contract.Events
{
    // گذار وضعیت قرارداد (پیش‌نویس/فعال/تکمیل/لغو/فسخ)
    public sealed record ContractStatusChangedEvent(
        long contractId,
        long caseId,
        long oldStatusId,
        long newStatusId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
