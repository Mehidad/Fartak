using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Supplier.Events
{
    // تغییر Meta تأمین‌کننده (jsonb آزاد)
    public sealed record SupplierMetaChangedEvent(
        long supplierId,
        string? oldMeta,
        string? newMeta) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
