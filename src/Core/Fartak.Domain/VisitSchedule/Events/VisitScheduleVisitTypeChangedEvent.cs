using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.VisitSchedule.Events
{
    // Events/VisitScheduleVisitTypeChangedEvent.cs — تغییر نوع بازدید روی رکورد باز
    public sealed record VisitScheduleVisitTypeChangedEvent(
        long visitScheduleId,
        long oldVisitTypeId,
        long newVisitTypeId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
