using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.TrackingSequence.Events
{
    // رزرو شماره رهگیری جدید برای پرونده — کد خروجی مستقیماً به Case.Create داده می‌شود
    public sealed record TrackingCodeReservedEvent(
        int jalaliYear,
        long seq,
        string trackingCode) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
