using Fartak.Domain.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Domain.Unit.Events
{
    // ایجاد واحد اندازه‌گیری جدید در جدول units
    public sealed record UnitCreatedEvent(
        long unitId,
        string code,
        string title,
        string? dimension) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
