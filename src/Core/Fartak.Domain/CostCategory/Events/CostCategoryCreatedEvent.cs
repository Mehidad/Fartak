using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CostCategory.Events
{
    // ایجاد سرفصل هزینه جدید (در ریشه یا زیر یک والد)
    public sealed record CostCategoryCreatedEvent(
        long costCategoryId,
        string code,
        string title,
        long? parentId,
        int sortOrder) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
