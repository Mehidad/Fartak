using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CaseStatus.Events
{
    // حذف فیزیکی گذار — لاگ‌های وضعیت به گذار ارجاع ندارند و آسیب نمی‌بینند
    public sealed record CaseStatusTransitionRemovedEvent(
        long caseStatusId,
        string code,
        long transitionId,
        long toStatusId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
