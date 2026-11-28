using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.ProcurementBatch
{
    // شناسه یک ردیف از جدول batch_lines (تخصیص هر خط برنامه به سفارش تجمیعی)
    public sealed record BatchLineId
    {
        public long Value { get; }

        private BatchLineId(long value) => Value = value;

        public static BatchLineId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "BatchLineId must be positive.");
            return new BatchLineId(value);
        }

        public static BatchLineId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
