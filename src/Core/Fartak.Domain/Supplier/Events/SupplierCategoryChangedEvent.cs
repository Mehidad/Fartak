using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Supplier.Events
{
    // تغییر دستهٔ فعالیت تأمین‌کننده
    public sealed record SupplierCategoryChangedEvent(
        long supplierId,
        string? oldCategory,
        string? newCategory) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
