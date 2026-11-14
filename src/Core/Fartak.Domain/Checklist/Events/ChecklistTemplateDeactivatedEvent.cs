using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Checklist.Events
{
    // غیرفعال‌سازی قالب چک‌لیست — ارجاع‌های بازدیدهای موجود معتبر می‌مانند
    public sealed record ChecklistTemplateDeactivatedEvent(
        long checklistTemplateId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
