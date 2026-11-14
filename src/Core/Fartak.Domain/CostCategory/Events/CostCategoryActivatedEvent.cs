using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CostCategory.Events
{
    // فعال‌سازی دوباره سرفصل غیرفعال هزینه
    public sealed record CostCategoryActivatedEvent(
        long costCategoryId,
        string code) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
