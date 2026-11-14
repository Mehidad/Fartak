using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.FieldVisit.Events
{
    // ویرایش مقدار یک پاسخ ثبت‌شده
    public sealed record VisitAnswerValueChangedEvent(
        long fieldVisitId,
        long answerId,
        long answerTypeId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
