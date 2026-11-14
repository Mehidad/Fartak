using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CaseStatus.Events
{
    // تغییر یا پاک‌شدن یادداشت توضیحی گذار
    public sealed record CaseStatusTransitionNoteChangedEvent(
        long caseStatusId,
        string code,
        long transitionId,
        long toStatusId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
