using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.VisitSchedule.Events
{
    // Events/VisitScheduleChecklistTemplateSetEvent.cs — اتصال/تعویض/حذف قالب چک‌لیست
    public sealed record VisitScheduleChecklistTemplateSetEvent(
        long visitScheduleId,
        long? checklistTemplateId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
