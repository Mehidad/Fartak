using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.ExpertProfile.Events
{
    // فعال‌سازی مجدد کارشناس — دوباره واجد شرایط تخصیص پرونده می‌شود
    public sealed record ExpertProfileActivatedEvent(
        long userId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
