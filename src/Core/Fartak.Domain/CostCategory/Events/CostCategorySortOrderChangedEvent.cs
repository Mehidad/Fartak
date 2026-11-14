using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CostCategory.Events
{
    // تغییر ترتیب نمایش سرفصل هزینه
    public sealed record CostCategorySortOrderChangedEvent(
        long costCategoryId,
        string code,
        int oldSortOrder,
        int newSortOrder) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
