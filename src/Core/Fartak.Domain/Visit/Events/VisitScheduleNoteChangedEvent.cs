using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Visit.Events
{
    // به‌روزرسانی یادداشت زمان‌بندی بازدید
    public sealed record VisitScheduleNoteChangedEvent(
        long visitScheduleId,
        long caseId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
