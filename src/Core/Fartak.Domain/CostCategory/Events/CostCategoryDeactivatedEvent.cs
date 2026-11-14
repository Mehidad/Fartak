using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CostCategory.Events
{
    // غیرفعال‌سازی سرفصل هزینه — ارجاع‌های موجود معتبر می‌مانند
    public sealed record CostCategoryDeactivatedEvent(
        long costCategoryId,
        string code) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
