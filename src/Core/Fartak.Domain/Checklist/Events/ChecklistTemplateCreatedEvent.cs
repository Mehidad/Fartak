using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Checklist.Events
{
    // ایجاد قالب چک‌لیست جدید (بدون آیتم)
    public sealed record ChecklistTemplateCreatedEvent(
        long checklistTemplateId,
        string title,
        long? cropId,
        long? visitTypeId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
