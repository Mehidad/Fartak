using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.User
{
    // شناسه کاربر (جدول users) — PK همه جدول‌های پروفایلی ۱:۱ (farmer_profiles / expert_profiles)
    // و FK ستون‌های created_by / expert_id / farmer_id در بقیه تجمع‌ها.
    // توجه: این فایل قبلاً هنگام پیست، namespace تودرتوی «Fartak.Domain.User.Fartak.Domain.User»
    // ساخته بود که با UserIdNamespaceCompat.cs خنثی شده است.
    public sealed record UserId
    {
        public long Value { get; }

        private UserId(long value) => Value = value;

        public static UserId From(long value)
        {
            if (value <= 0)
                throw new DomainValidationException(ErrorCodes.InvalidId, "UserId must be positive.");
            return new UserId(value);
        }

        public static UserId Empty => new(0);

        public override string ToString() => Value.ToString();
    }
}
