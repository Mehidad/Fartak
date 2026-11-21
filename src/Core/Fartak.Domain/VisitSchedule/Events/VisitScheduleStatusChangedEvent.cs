using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.VisitSchedule.Events
{
    // Events/VisitScheduleStatusChangedEvent.cs — گذار وضعیت چرخه حیات؛
    // کد وضعیت مقصد برای Handlerها (مثلاً لغو یادآور روی done/canceled/expired)
    public sealed record VisitScheduleStatusChangedEvent(
     long visitScheduleId,
     long caseId,
     long fromStatusId,
     long toStatusId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
