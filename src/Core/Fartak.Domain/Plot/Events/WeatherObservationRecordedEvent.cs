using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Plot.Events
{
    // ثبت مشاهده هواشناسی برای قطعه — مبنای هشدارهای سرمازدگی/تنش گرمایی در لایه Application
    public sealed record WeatherObservationRecordedEvent(
        long plotId,
        long weatherObservationId,
        DateTime observedAt) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
