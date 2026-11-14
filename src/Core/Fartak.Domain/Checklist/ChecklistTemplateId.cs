using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Checklist
{
    // شناسه یک ردیف از جدول checklist_templates (قالب چک‌لیست بازدید)
    public sealed record ChecklistTemplateId
    {
        public long Value { get; }

        private ChecklistTemplateId(long value) => Value = value;

        public static ChecklistTemplateId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "ChecklistTemplateId must be positive.");
            return new ChecklistTemplateId(value);
        }

        public static ChecklistTemplateId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
