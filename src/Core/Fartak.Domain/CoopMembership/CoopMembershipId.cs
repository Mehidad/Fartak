using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CoopMembership
{
    // شناسه یک ردیف از جدول coop_memberships (عضویت تعاونی روستایی کشاورز)
    public sealed record CoopMembershipId
    {
        public long Value { get; }

        private CoopMembershipId(long value) => Value = value;

        public static CoopMembershipId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "CoopMembershipId must be positive.");
            return new CoopMembershipId(value);
        }

        public static CoopMembershipId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
