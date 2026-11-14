using Fartak.Domain.Framework;

namespace Fartak.Domain.Catalog.Events
{
    // فایل Events/CropVarietyRenamedEvent.cs
    public sealed record CropVarietyRenamedEvent(
        long cropId,
        long varietyId,
        string code,
        string oldTitle,
        string newTitle) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
