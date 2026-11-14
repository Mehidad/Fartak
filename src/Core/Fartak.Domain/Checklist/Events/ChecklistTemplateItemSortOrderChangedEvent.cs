using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Checklist.Events
{
    // تغییر ترتیب نمایش آیتم در فرم بازدید
    public sealed record ChecklistTemplateItemSortOrderChangedEvent(
        long checklistTemplateId,
        long itemId,
        int oldSortOrder,
        int newSortOrder) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
