using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CoopMembership.Events
{
    // ثبت عضویت تعاونی جدید برای کشاورز
    public sealed record CoopMembershipCreatedEvent(
        long coopMembershipId,
        long farmerUserId,
        string coopTitle,
        string memberNo,
        DateTime? joinedAt) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
