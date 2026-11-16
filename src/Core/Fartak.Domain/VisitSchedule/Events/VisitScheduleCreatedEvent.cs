using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.VisitSchedule.Events
{
    // Events/VisitScheduleCreatedEvent.cs — ثبت زمان‌بندی جدید برای پرونده
    public sealed record VisitScheduleCreatedEvent(
        long visitScheduleId,
        long caseId,
        long visitTypeId,
        DateTime scheduledFor) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
