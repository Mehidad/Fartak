using Fartak.Domain.Framework;

namespace Fartak.Domain.Catalog.Events
{
    // فایل Events/CropActivatedEvent.cs
    public sealed record CropActivatedEvent(long cropId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
