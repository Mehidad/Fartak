using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.FarmerProfile.Events
{
    // ایجاد پروفایل کشاورز برای کاربر (رابطه ۱:۱ — شناسه تجمع همان UserId است)
    public sealed record FarmerProfileCreatedEvent(
        long userId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
