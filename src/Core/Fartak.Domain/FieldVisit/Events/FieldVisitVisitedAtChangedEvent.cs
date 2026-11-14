using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.FieldVisit.Events
{
    // اصلاح زمان انجام بازدید
    public sealed record FieldVisitVisitedAtChangedEvent(
        long fieldVisitId,
        DateTime oldVisitedAt,
        DateTime newVisitedAt) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
