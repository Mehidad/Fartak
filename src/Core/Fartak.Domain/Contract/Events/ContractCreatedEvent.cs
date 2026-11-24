using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Contract.Events
{
    // عقد قرارداد جدید در چارچوب پرونده
    public sealed record ContractCreatedEvent(
        long contractId,
        long caseId,
        long farmerId,
        long? expertId,
        string title,
        long totalAmountRial,
        decimal? commissionRate,
        long statusId,
        DateOnly? startDate,
        DateOnly? endDate) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
