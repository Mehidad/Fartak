using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Visit.Events
{
    // تغییر زمان بازدید — یادآورهای قبلی باید بازتنظیم شوند (کار لایه Application)
    public sealed record VisitScheduleRescheduledEvent(
        long visitScheduleId,
        long caseId,
        DateTime oldScheduledFor,
        DateTime newScheduledFor) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
