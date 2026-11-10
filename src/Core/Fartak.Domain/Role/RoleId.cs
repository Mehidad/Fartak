using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Role
{
    public sealed record RoleId
    {
        public long Value { get; }

        private RoleId(long value) => Value = value;

        public static RoleId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "RoleId must be positive.");
            return new RoleId(value);
        }

        public static RoleId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
