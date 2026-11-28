using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Supplier.Events
{
    // به‌روزرسانی اطلاعات تماس تأمین‌کننده (تلفن و نام رابط)
    public sealed record SupplierContactUpdatedEvent(
        long supplierId,
        string? oldPhone,
        string? newPhone,
        string? oldContactName,
        string? newContactName) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
