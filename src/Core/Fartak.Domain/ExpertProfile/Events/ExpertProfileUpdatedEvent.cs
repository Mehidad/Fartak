using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.ExpertProfile.Events
{
    // به‌روزرسانی پروفایل کارشناس
    public sealed record ExpertProfileUpdatedEvent(
        long userId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
