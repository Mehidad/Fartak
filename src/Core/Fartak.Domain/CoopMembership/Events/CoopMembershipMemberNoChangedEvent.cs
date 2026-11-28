using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CoopMembership.Events
{
    // اصلاح شماره عضویت
    public sealed record CoopMembershipMemberNoChangedEvent(
        long coopMembershipId,
        long farmerUserId,
        string oldMemberNo,
        string newMemberNo) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
