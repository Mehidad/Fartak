using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CostCategory.Events
{
    // تغییر عنوان نمایشی سرفصل هزینه
    public sealed record CostCategoryRenamedEvent(
        long costCategoryId,
        string code,
        string oldTitle,
        string newTitle) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
