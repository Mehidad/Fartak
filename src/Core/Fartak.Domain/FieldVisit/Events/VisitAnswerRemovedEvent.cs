using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.FieldVisit.Events
{
    // حذف فیزیکی پاسخ از بازدید
    public sealed record VisitAnswerRemovedEvent(
        long fieldVisitId,
        long answerId,
        long? templateItemId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
