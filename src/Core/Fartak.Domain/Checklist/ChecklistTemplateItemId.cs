using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Checklist
{
    // شناسه یک ردیف از جدول checklist_template_items (آیتم‌های چک‌لیست)؛
    // پاسخ‌های ثبت‌شده بازدیدها (field_visit_items آینده) با همین شناسه به آیتم ارجاع می‌دهند
    public sealed record ChecklistTemplateItemId
    {
        public long Value { get; }

        private ChecklistTemplateItemId(long value) => Value = value;

        public static ChecklistTemplateItemId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "ChecklistTemplateItemId must be positive.");
            return new ChecklistTemplateItemId(value);
        }

        public static ChecklistTemplateItemId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
