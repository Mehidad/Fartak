using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.FieldVisit.Events
{
    // تغییر مرحلهٔ رشد مشاهده‌شده در بازدید
    public sealed record FieldVisitGrowthStageChangedEvent(
        long fieldVisitId,
        long? oldGrowthStageId,
        long? newGrowthStageId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
