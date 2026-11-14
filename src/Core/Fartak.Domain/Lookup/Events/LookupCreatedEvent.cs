using Fartak.Domain.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Domain.Lookup.Events
{
    // ایجاد مقدار جدید در فهرست‌های پایه سیستم (جدول lookups)
    public sealed record LookupCreatedEvent(
        long lookupId,
        string domain,
        string code,
        string title,
        int sortOrder) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
