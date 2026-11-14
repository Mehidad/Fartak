using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.FieldVisit.Events
{
    // ویرایش متن پرسش آزاد (فقط پاسخ‌های بدون آیتم قالب)
    public sealed record VisitAnswerQuestionChangedEvent(
        long fieldVisitId,
        long answerId,
        string? oldQuestionText,
        string? newQuestionText) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
