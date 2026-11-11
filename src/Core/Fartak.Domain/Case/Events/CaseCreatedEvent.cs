using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Case.Events
{
    // ثبت پرونده جدید (یک قطعه × یک محصول × یک سال زراعی)
    public sealed record CaseCreatedEvent(
        long caseId,
        string trackingCode,
        long farmerId,
        long plotId,
        long cropId,
        long? varietyId,
        int seasonYear,
        decimal areaHectares) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
