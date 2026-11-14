using Fartak.Domain.Framework;

namespace Fartak.Domain.Catalog.Events
{
    // فایل Events/CropVarietyActivatedEvent.cs
    public sealed record CropVarietyActivatedEvent(
        long cropId,
        long varietyId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
