using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Supplier.Events
{
    // ثبت تأمین‌کننده جدید در کاتالوگ تأمین‌کنندگان
    public sealed record SupplierCreatedEvent(
        long supplierId,
        string title,
        string? phone,
        string? contactName,
        string? category,
        string? meta) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
