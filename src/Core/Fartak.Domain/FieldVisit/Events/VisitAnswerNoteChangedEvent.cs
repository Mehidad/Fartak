using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.FieldVisit.Events
{
    // ویرایش/پاک‌کردن یادداشت یک پاسخ
    public sealed record VisitAnswerNoteChangedEvent(
        long fieldVisitId,
        long answerId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
