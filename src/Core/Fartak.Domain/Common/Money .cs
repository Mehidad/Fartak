using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Common
{
    public sealed class Money : ValueObject
    {
        public decimal Amount { get; private set; }
        public string Currency { get; private set; } // "IRR", "USD"

        private Money(decimal amount, string currency)
        {
            if (amount < 0)
                throw new ArgumentException("مبلغ نمی‌تواند منفی باشد", nameof(amount));

            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("واحد پول نامعتبر است", nameof(currency));

            Amount = amount;
            Currency = currency;
        }

        public static Money Create(decimal amount, string currency = "IRR")
        {
            return new Money(amount, currency);
        }

        public Money Add(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException("نمی‌توان دو واحد پولی متفاوت را جمع کرد");

            return new Money(Amount + other.Amount, Currency);
        }

        public Money Subtract(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException("نمی‌توان دو واحد پولی متفاوت را تفریق کرد");

            return new Money(Amount - other.Amount, Currency);
        }

        public Money Multiply(decimal factor)
        {
            return new Money(Amount * factor, Currency);
        }

        public static bool operator >(Money left, Money right)
        {
            if (left.Currency != right.Currency)
                throw new InvalidOperationException("نمی‌توان دو واحد پولی متفاوت را مقایسه کرد");

            return left.Amount > right.Amount;
        }

        public static bool operator <(Money left, Money right)
        {
            if (left.Currency != right.Currency)
                throw new InvalidOperationException("نمی‌توان دو واحد پولی متفاوت را مقایسه کرد");

            return left.Amount < right.Amount;
        }

        public static bool operator >=(Money left, Money right) => !(left < right);
        public static bool operator <=(Money left, Money right) => !(left > right);

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }

        public override string ToString() => $"{Amount:N0} {Currency}";
    }
}
