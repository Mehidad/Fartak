using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Plot.Events
{
    // غیرفعال‌سازی قطعه زمین (حذف منطقی — سابقه پرونده‌ها حفظ می‌شود)
    public sealed record PlotDeactivatedEvent(
        long plotId,
        long farmerUserId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
