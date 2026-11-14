using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.FieldVisit
{
    // شناسه یک ردیف از جدول visit_schedules (زمان‌بندی بازدید).
    // همین حالا در تجمع FieldVisit برای ستون visit_schedule_id استفاده می‌شود و
    // در گام بعدی تجمع «VisitSchedule» نیز بدون تغییر از همین شناسه استفاده خواهد کرد
    // (همان الگوی CaseStatusId که در پوشهٔ Case ساخته شد و تجمع CaseStatus بازمصرف کرد).
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
