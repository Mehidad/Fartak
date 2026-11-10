using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Plot.Events
{
    // ثبت نتیجه تست خاک برای قطعه
    public sealed record SoilTestRecordedEvent(
        long plotId,
        long soilTestId,
        DateOnly testedAt) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
