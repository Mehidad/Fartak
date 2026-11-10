using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Region.Events
{
    // جابه‌جایی منطقه به والد دیگر (بازسازی درخت)
    public sealed record RegionParentChangedEvent(
        long regionId,
        long? oldParentId,
        long? newParentId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
