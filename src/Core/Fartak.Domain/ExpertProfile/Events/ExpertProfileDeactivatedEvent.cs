using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.ExpertProfile.Events
{
    // غیرفعال‌سازی کارشناس — برای بازپس‌گیری پرونده‌های در جریان در Application مصرف می‌شود
    public sealed record ExpertProfileDeactivatedEvent(
        long userId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
