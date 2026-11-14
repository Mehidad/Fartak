using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.FieldVisit.Events
{
    // ثبت بازدید میدانی جدید برای پرونده
    public sealed record FieldVisitCreatedEvent(
        long fieldVisitId,
        long caseId,
        long expertUserId,
        long? visitScheduleId,
        DateTime visitedAt,
        long? growthStageId,
        string? overallSummary) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
