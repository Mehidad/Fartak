using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.TrackingSequence.Events
{
    // ایجاد ردیف شمارنده سال جدید (با اولین پرونده سال یا هنگام درون‌ریزی داده تاریخی)
    public sealed record TrackingSequenceCreatedEvent(
        int jalaliYear,
        long lastSeq) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
