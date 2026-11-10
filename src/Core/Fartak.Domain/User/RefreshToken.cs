using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Domain.User
{
    public class RefreshToken
    {
        public string Token { get; private set; }
        public DateTime ExpiredAt { get; private set; }
        public bool IsRevoked { get; private set; }

        private RefreshToken() { }

        public RefreshToken(string token, DateTime expiredAt, bool isRevoked)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new NullReferenceException("توکن نمی تواند خالی باشد .");

            Token = token;
            ExpiredAt = expiredAt;
            IsRevoked = isRevoked;
        }

        public bool IsValid() => !IsRevoked && DateTime.UtcNow < ExpiredAt;

        public void Revoke() => IsRevoked = true;
    }
}
