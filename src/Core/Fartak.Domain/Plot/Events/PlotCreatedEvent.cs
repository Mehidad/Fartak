using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Plot.Events
{
    // ثبت قطعه زمین جدید توسط کشاورز
    public sealed record PlotCreatedEvent(
        long plotId,
        long farmerUserId,
        string title,
        decimal areaHectares) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
