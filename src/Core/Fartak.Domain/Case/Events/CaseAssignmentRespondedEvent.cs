using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Case.Events
{
    // ثبت نتیجه پاسخ کارشناس به تخصیص (پذیرش/ردی/...) — بستن SLA
    public sealed record CaseAssignmentRespondedEvent(
        long caseId,
        long assignmentId,
        long expertId,
        long outcomeId,
        DateTime respondedAt) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
