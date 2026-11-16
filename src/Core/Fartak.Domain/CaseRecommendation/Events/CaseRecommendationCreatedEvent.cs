using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CaseRecommendation.Events
{
    // Events/CaseRecommendationCreatedEvent.cs — ثبت توصیه؛ فلگ‌های مخاطب برای
    // Handler اعلان کنار رویداد می‌آیند تا همان لحظهٔ ایجاد، مخاطبان معلوم باشند
    public sealed record CaseRecommendationCreatedEvent(
        long caseRecommendationId,
        long caseId,
        long authorId,
        long? visitId,
        bool notifyManager,
        bool notifyFarmer) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
