using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.FieldVisit.Events
{
    // ویرایش جمع‌بندی کلی بازدید
    public sealed record FieldVisitSummaryChangedEvent(
        long fieldVisitId,
        string? oldSummary,
        string? newSummary) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
