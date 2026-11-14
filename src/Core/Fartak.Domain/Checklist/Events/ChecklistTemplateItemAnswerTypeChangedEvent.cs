using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Checklist.Events
{
    // تغییر نوع پاسخ آیتم (مقیاس/بله-خیر/عددی/متنی)
    public sealed record ChecklistTemplateItemAnswerTypeChangedEvent(
        long checklistTemplateId,
        long itemId,
        long oldAnswerTypeId,
        long newAnswerTypeId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
