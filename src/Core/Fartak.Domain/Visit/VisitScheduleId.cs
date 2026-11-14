using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Visit
{
    // شناسه یک ردیف از جدول visit_schedules (زمان‌بندی بازدید)؛
    // یادآورهای کارشناس (reminders آینده) و بازدید میدانی (field_visits.visit_schedule_id) به آن ارجاع می‌دهند
    public sealed record VisitScheduleId
    {
        public long Value { get; }

        private VisitScheduleId(long value) => Value = value;

        public static VisitScheduleId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "VisitScheduleId must be positive.");
            return new VisitScheduleId(value);
        }

        public static VisitScheduleId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
