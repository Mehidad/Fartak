using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Supplier.Events
{
    // فعال‌سازی مجدد تأمین‌کننده
    public sealed record SupplierActivatedEvent(
        long supplierId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
