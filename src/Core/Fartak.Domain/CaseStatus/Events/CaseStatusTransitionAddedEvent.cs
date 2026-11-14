using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CaseStatus.Events
{
    // تعریف گذار خروجی جدید از این وضعیت به وضعیت مقصد
    public sealed record CaseStatusTransitionAddedEvent(
        long caseStatusId,
        string code,
        long transitionId,
        long toStatusId,
        bool isAutomatic) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
