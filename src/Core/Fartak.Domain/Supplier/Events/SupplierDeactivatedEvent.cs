using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Supplier.Events
{
    // غیرفعال‌سازی تأمین‌کننده — برای سفارش تجمیعی جدید انتخاب نمی‌شود
    public sealed record SupplierDeactivatedEvent(
        long supplierId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
