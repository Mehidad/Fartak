using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.ProcurementBatch.Events
{
    // تخصیص خط برنامه به سفارش تجمیعی
    public sealed record BatchLineAddedEvent(
        long batchId,
        long batchLineId,
        long planLineId,
        decimal quantity,
        long? unitId,
        long? unitPriceRial,
        long? lineTotalRial,
        string? note) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
