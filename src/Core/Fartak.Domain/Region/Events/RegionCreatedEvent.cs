using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Region.Events
{
    // ثبت منطقه جدید در سلسله‌مراتب جغرافیایی
    public sealed record RegionCreatedEvent(
        long regionId,
        RegionLevel level,
        string name,
        long? parentId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
