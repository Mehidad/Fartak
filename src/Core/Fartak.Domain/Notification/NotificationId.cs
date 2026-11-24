using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Notification
{
    // شناسه یک ردیف از جدول notifications (اعلان‌های سیستمی به کاربر)
    public sealed record NotificationId
    {
        public long Value { get; }

        private NotificationId(long value) => Value = value;

        public static NotificationId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "NotificationId must be positive.");
            return new NotificationId(value);
        }

        public static NotificationId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
