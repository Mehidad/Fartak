using Fartak.Domain.Framework;

namespace Fartak.Domain.Catalog.Events
{
    // فایل Events/CropVarietyDeactivatedEvent.cs
    public sealed record CropVarietyDeactivatedEvent(
        long cropId,
        long varietyId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
