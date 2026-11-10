using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.User
{

    namespace Fartak.Domain.User
    {

        public sealed record UserId
        {
            public long Value { get; }

            private UserId(long value) => Value = value;

            public static UserId From(long value)
            {
                if (value <= 0)
                    throw new DomainValidationException(ErrorCodes.InvalidId, "UserId must be positive.");
                return new UserId(value);
            }

            public static UserId Empty => new(0);

            public override string ToString() => Value.ToString();
        }
    }
}

