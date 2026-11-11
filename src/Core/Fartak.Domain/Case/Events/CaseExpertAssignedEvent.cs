using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Case.Events
{
    // تخصیص پرونده به کارشناس با مهلت پاسخ (SLA)
    public sealed record CaseExpertAssignedEvent(
        long caseId,
        long assignmentId,
        long expertId,
        DateTime deadline) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
