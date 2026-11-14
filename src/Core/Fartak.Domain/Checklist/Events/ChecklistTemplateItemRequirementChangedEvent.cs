using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Checklist.Events
{
    // تغییر اجباری/اختیاری‌بودن پاسخ آیتم
    public sealed record ChecklistTemplateItemRequirementChangedEvent(
        long checklistTemplateId,
        long itemId,
        bool isRequired) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
