using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Checklist.Events
{
    // تغییر یا پاک‌شدن واحد آیتم (oldUnitId پس از پاک‌سازی null است)
    public sealed record ChecklistTemplateItemUnitChangedEvent(
        long checklistTemplateId,
        long itemId,
        long? oldUnitId,
        long? newUnitId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
