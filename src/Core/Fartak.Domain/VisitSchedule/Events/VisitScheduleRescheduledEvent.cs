using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.VisitSchedule.Events
{
    // Events/VisitScheduleRescheduledEvent.cs — جابه‌جایی زمان بازدید (مبنای به‌روزرسانی یادآور)
    public sealed record VisitScheduleRescheduledEvent(
      long visitScheduleId,
      long caseId,
      DateTime oldScheduledFor,
      DateTime newScheduledFor) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
