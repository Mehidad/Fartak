using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Case.Events
{
    // تعیین/اصلاح تاریخ کشت پرونده
    public sealed record CasePlantingDateSetEvent(
        long caseId,
        DateOnly? plantingDate) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
