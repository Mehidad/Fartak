using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Visit.Events
{
    // تغییر یا پاک‌شدن قالب چک‌لیست بازدید (oldTemplateId پس از پاک‌سازی null است)
    public sealed record VisitScheduleChecklistTemplateChangedEvent(
        long visitScheduleId,
        long caseId,
        long? oldChecklistTemplateId,
        long? newChecklistTemplateId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
