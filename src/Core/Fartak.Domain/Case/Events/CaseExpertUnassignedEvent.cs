using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Case.Events
{
    // خروج کارشناس از پرونده — بازگشت پرونده به صف تخصیص
    public sealed record CaseExpertUnassignedEvent(
        long caseId,
        long expertId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
