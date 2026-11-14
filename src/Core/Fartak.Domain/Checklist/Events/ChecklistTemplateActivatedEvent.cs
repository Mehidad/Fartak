using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Checklist.Events
{
    // فعال‌سازی دوباره قالب غیرفعال چک‌لیست
    public sealed record ChecklistTemplateActivatedEvent(
        long checklistTemplateId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
