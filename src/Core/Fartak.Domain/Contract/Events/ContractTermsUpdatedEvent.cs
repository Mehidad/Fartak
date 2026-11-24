using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Contract.Events
{
    // ویرایش شرایط قرارداد (عنوان، مبلغ کل، کارمزد، بازهٔ اجرا)
    public sealed record ContractTermsUpdatedEvent(
        long contractId,
        long caseId,
        string title,
        long totalAmountRial,
        decimal? commissionRate,
        DateOnly? startDate,
        DateOnly? endDate) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
