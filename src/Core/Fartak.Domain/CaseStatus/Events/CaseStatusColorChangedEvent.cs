using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CaseStatus.Events
{
    // تغییر یا پاک‌شدن رنگ نمایش وضعیت (oldColor پس از پاک‌سازی null است)
    public sealed record CaseStatusColorChangedEvent(
        long caseStatusId,
        string code,
        string? oldColor,
        string? newColor) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
