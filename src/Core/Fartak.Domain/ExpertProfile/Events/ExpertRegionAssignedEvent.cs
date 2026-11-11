using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.ExpertProfile.Events
{
    // افزودن منطقه به مناطق تحت پوشش کارشناس
    public sealed record ExpertRegionAssignedEvent(
        long userId,
        long regionId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
