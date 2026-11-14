using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Checklist.Events
{
    // تغییر عنوان نمایشی قالب چک‌لیست
    public sealed record ChecklistTemplateRenamedEvent(
        long checklistTemplateId,
        string oldTitle,
        string newTitle) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
