using Fartak.Domain.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Domain.Unit.Events
{
    // تغییر یا پاک‌شدن بُعد اندازه‌گیری واحد (oldDimension پس از پاک‌سازی null است)
    public sealed record UnitDimensionChangedEvent(
        long unitId,
        string code,
        string? oldDimension,
        string? newDimension) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
