using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Checklist.Events
{
    // حذف فیزیکی آیتم — فقط آیتم بدون پاسخ ثبت‌شده (بررسی در لایه Application)
    public sealed record ChecklistTemplateItemRemovedEvent(
        long checklistTemplateId,
        long itemId,
        string itemTitle) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
