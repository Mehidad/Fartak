using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CaseStatus.Events
{
    // تغییر ترتیب نمایش وضعیت پرونده
    public sealed record CaseStatusSortOrderChangedEvent(
        long caseStatusId,
        string code,
        int oldSortOrder,
        int newSortOrder) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
