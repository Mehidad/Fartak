using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.ProcurementBatch.Events
{
    // ثبت پاسخ قیمت واحد تأمین‌کننده روی ردیف سفارش + بازمحاسبهٔ جمع ردیف
    public sealed record BatchLinePriceUpdatedEvent(
        long batchId,
        long batchLineId,
        long planLineId,
        long? oldUnitPriceRial,
        long? newUnitPriceRial,
        long? lineTotalRial) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
