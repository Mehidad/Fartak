using Fartak.Domain.Framework;

namespace Fartak.Domain.Catalog.Events
{
    // فایل Events/CropVarietyAddedEvent.cs
    public sealed record CropVarietyAddedEvent(
        long cropId,
        long varietyId,
        string code,
        string title) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
