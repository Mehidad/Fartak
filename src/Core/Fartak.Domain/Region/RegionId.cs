using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Region
{
    public sealed record RegionId
    {
        public long Value { get; }

        private RegionId(long value) => Value = value;

        public static RegionId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "RegionId must be positive.");
            return new RegionId(value);
        }

        public static RegionId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
