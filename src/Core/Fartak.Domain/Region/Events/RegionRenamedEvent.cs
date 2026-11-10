using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Region.Events
{
    // تغییر نام نمایشی منطقه
    public sealed record RegionRenamedEvent(
        long regionId,
        string oldName,
        string newName) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
