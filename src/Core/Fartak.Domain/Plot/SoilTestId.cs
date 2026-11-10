using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Plot
{
    public sealed record SoilTestId
    {
        public long Value { get; }

        private SoilTestId(long value) => Value = value;

        public static SoilTestId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "SoilTestId must be positive.");
            return new SoilTestId(value);
        }

        public static SoilTestId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
