using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CaseRecommendation.Events
{
    // Events/CaseRecommendationVisitSetEvent.cs — اتصال توصیه به بازدید مبدأ یا حذف اتصال (null)
    public sealed record CaseRecommendationVisitSetEvent(
        long caseRecommendationId,
        long? visitId) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
