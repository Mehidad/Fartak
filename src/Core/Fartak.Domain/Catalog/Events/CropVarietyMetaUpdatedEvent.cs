using Fartak.Domain.Framework;

namespace Fartak.Domain.Catalog.Events
{
    // فایل Events/CropVarietyMetaUpdatedEvent.cs
    public sealed record CropVarietyMetaUpdatedEvent(
        long cropId,
        long varietyId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
