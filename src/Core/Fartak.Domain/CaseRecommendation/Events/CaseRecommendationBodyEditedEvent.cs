using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CaseRecommendation.Events
{
    // Events/CaseRecommendationBodyEditedEvent.cs — ویرایش متن توصیه
    // (متن قدیم/جدید در رویداد نمی‌آید — Handler از read model می‌خواند؛ مثل CaseNoteAddedEvent)
    public sealed record CaseRecommendationBodyEditedEvent(
        long caseRecommendationId,
        long caseId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
