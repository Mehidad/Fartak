using Fartak.Domain.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Domain.Catalog.Events
{
    // فایل Events/CropCreatedEvent.cs
    public sealed record CropCreatedEvent(
        long cropId,
        string code,
        string title) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
