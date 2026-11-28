using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.ProcurementBatch
{
    // شناسه یک ردیف از جدول procurement_batches (سفارش تجمیعی از خطوط برنامه چند پرونده)
    public sealed record ProcurementBatchId
    {
        public long Value { get; }

        private ProcurementBatchId(long value) => Value = value;

        public static ProcurementBatchId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "ProcurementBatchId must be positive.");
            return new ProcurementBatchId(value);
        }

        public static ProcurementBatchId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
