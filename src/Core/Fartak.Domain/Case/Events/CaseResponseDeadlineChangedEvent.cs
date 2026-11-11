using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Case.Events
{
    // تغییر مهلت پاسخ کارشناس (تمدید یا تقلیل SLA جاری پرونده)
    public sealed record CaseResponseDeadlineChangedEvent(
        long caseId,
        DateTime oldDeadline,
        DateTime newDeadline) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
