using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CaseRecommendation
{
    // شناسه یک ردیف از جدول case_recommendations (توصیه کارشناس روی پرونده)
    // در Aggregate «CaseRecommendation» (CaseRecommendation.cs) استفاده می‌شود
    public sealed record CaseRecommendationId
    {
        public long Value { get; }

        private CaseRecommendationId(long value) => Value = value;

        public static CaseRecommendationId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "CaseRecommendationId must be positive.");
            return new CaseRecommendationId(value);
        }

        public static CaseRecommendationId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
