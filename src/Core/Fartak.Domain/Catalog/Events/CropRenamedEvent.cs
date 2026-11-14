using Fartak.Domain.Framework;

namespace Fartak.Domain.Catalog.Events
{
    // فایل Events/CropRenamedEvent.cs
    public sealed record CropRenamedEvent(
        long cropId,
        string oldTitle,
        string newTitle) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
