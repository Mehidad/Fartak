using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.ProcurementBatch.Events
{
    // حذف ردیف سفارش تجمیعی
    public sealed record BatchLineRemovedEvent(
        long batchId,
        long batchLineId,
        long planLineId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
