using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Common
{
    public sealed class OTP : ValueObject
    {
        public string Code { get; }
        public DateTime ExpiredAt { get; }

        private OTP()
        {
        }

        private OTP(string code, DateTime expiredAt)
        {
            Code = code;
            ExpiredAt = expiredAt;
        }

        public static OTP Generate()
        {
            string code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            DateTime expiredAt = DateTime.UtcNow.AddMinutes(2);

            return new OTP(code, expiredAt);
        }

        public bool IsExpired() => DateTime.UtcNow > ExpiredAt;

        // اصلاح: قبلاً || بود که هر کدی وقتی منقضی نشده بود قبول می‌شد
        public bool IsValid(string code) => code == Code && !IsExpired();

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Code;
            yield return ExpiredAt;
        }
    }
}
