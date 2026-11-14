using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.FieldVisit.Events
{
    // ثبت پاسخ آیتم چک‌لیست (یا پرسش آزاد) در بازدید
    public sealed record VisitAnswerAddedEvent(
        long fieldVisitId,
        long answerId,
        long? templateItemId,
        long answerTypeId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
