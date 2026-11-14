using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Visit.Events
{
    // تغییر وضعیت زمان‌بندی بازدید (لغو/به‌تعویق/انجام‌شده و ...)
    public sealed record VisitScheduleStatusChangedEvent(
        long visitScheduleId,
        long caseId,
        long oldStatusId,
        long newStatusId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
