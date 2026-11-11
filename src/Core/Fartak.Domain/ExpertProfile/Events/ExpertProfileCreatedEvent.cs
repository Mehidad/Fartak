using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.ExpertProfile.Events
{
    // ایجاد پروفایل کارشناس برای کاربر (رابطه ۱:۱ — شناسه تجمع همان UserId است)
    public sealed record ExpertProfileCreatedEvent(
        long userId,
        int maxActiveCases,
        int maxDailyVisits) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
