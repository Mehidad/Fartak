using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.FieldVisit.Events
{
    // تغییر اتصال بازدید به برنامهٔ بازدید مادر
    public sealed record FieldVisitScheduleChangedEvent(
        long fieldVisitId,
        long? oldVisitScheduleId,
        long? newVisitScheduleId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
