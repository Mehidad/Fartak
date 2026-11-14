using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CostCategory.Events
{
    // جابه‌جایی سرفصل به والد دیگر (oldParentId پس از تبدیل به ریشه null است)
    public sealed record CostCategoryParentChangedEvent(
        long costCategoryId,
        string code,
        long? oldParentId,
        long? newParentId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
