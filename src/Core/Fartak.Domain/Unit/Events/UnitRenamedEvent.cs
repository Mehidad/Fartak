using Fartak.Domain.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Domain.Unit.Events
{
    // تغییر عنوان نمایشی واحد اندازه‌گیری
    public sealed record UnitRenamedEvent(
        long unitId,
        string code,
        string oldTitle,
        string newTitle) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
