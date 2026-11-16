using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CaseRecommendation.Events
{
    // Events/CaseRecommendationAudienceChangedEvent.cs — تغییر مخاطبان اعلان؛
    // مقادیر جدید برای تصمیم Handler دربارهٔ اعلان تکمیلی/لغو ارسال قبلی
    public sealed record CaseRecommendationAudienceChangedEvent(
        long caseRecommendationId,
        bool notifyManager,
        bool notifyFarmer) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
