using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CoopMembership.Events
{
    // اصلاح تاریخ عضویت
    public sealed record CoopMembershipJoinedAtChangedEvent(
        long coopMembershipId,
        long farmerUserId,
        DateTime? oldJoinedAt,
        DateTime? newJoinedAt) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
