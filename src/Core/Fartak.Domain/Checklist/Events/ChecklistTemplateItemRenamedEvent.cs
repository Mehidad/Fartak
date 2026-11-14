using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Checklist.Events
{
    // تغییر عنوان آیتم چک‌لیست (یکتایی نسبت به سایر آیتم‌های قالب حفظ می‌شود)
    public sealed record ChecklistTemplateItemRenamedEvent(
        long checklistTemplateId,
        long itemId,
        string oldTitle,
        string newTitle) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
