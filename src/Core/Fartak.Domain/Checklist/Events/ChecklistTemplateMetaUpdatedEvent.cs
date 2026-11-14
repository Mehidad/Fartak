using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Checklist.Events
{
    // به‌روزرسانی Meta (jsonb آزاد) قالب چک‌لیست
    public sealed record ChecklistTemplateMetaUpdatedEvent(
        long checklistTemplateId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
