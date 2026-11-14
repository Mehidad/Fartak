using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Checklist.Events
{
    // افزودن آیتم به قالب چک‌لیست (فقط در قالب فعال)
    public sealed record ChecklistTemplateItemAddedEvent(
        long checklistTemplateId,
        long itemId,
        string itemTitle,
        long answerTypeId,
        bool isRequired) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
