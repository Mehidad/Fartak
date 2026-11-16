using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.VisitSchedule.Events
{
    // Events/VisitScheduleNoteChangedEvent.cs — تغییر یادداشت هماهنگی
    public sealed record VisitScheduleNoteChangedEvent(
        long visitScheduleId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
