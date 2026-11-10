using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Plot.Events
{
    // فعال‌سازی مجدد قطعه زمین
    public sealed record PlotActivatedEvent(
        long plotId,
        long farmerUserId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
