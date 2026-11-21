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
        long? checklistTemplateId,
        DateTime scheduledFor,
        long statusId,
        long? createdBy) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
