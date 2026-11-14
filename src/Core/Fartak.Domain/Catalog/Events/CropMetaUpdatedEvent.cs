using Fartak.Domain.Framework;

namespace Fartak.Domain.Catalog.Events
{
    // فایل Events/CropMetaUpdatedEvent.cs
    public sealed record CropMetaUpdatedEvent(long cropId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
