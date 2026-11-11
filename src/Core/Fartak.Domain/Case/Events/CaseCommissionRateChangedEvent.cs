using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Case.Events
{
    // تعیین کارمزد اختصاصی پرونده؛ مقدار تهی یعنی بازگشت به پیش‌فرض تنظیمات
    public sealed record CaseCommissionRateChangedEvent(
        long caseId,
        decimal? commissionRate) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
