using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.CostEntry.Events
{
    // اصلاح مبلغ هزینه — مبنای بازمحاسبهٔ انحراف بودجه در Application
    public sealed record CostEntryAmountUpdatedEvent(
        long costEntryId,
        long caseId,
        long? planLineId,
        long oldAmountRial,
        long newAmountRial) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
