using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CaseStatus.Events
{
    // ایجاد وضعیت جدید چرخه پرونده (بدون هیچ گذار خروجی)
    public sealed record CaseStatusCreatedEvent(
        long caseStatusId,
        string code,
        string title,
        string groupCode,
        int sortOrder,
        string? color) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
