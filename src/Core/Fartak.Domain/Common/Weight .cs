using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Common
{
    public sealed class Weight : ValueObject
    {
        public decimal Value { get; private set; }
        public string Unit { get; private set; } // "KG", "TON"

        private Weight(decimal value, string unit)
        {
            if (value < 0)
                throw new ArgumentException("وزن نمی‌تواند منفی باشد", nameof(value));

            if (string.IsNullOrWhiteSpace(unit))
                throw new ArgumentException("واحد وزن نامعتبر است", nameof(unit));

            Value = value;
            Unit = unit.ToUpperInvariant();
        }

        public static Weight Create(decimal value, string unit = "KG")
        {
            return new Weight(value, unit);
        }

        public Weight Add(Weight other)
        {
            var normalizedOther = other.ConvertTo(Unit);
            return new Weight(Value + normalizedOther.Value, Unit);
        }

        public Weight Subtract(Weight other)
        {
            var normalizedOther = other.ConvertTo(Unit);
            if (Value < normalizedOther.Value)
                throw new InvalidOperationException("وزن نمی‌تواند منفی شود");

            return new Weight(Value - normalizedOther.Value, Unit);
        }

        public Weight ConvertTo(string targetUnit)
        {
            if (Unit == targetUnit)
                return this;

            decimal convertedValue = (Unit, targetUnit.ToUpperInvariant()) switch
            {
                ("KG", "TON") => Value / 1000m,
                ("TON", "KG") => Value * 1000m,
                _ => throw new InvalidOperationException($"تبدیل از {Unit} به {targetUnit} پشتیبانی نمی‌شود")
            };

            return new Weight(convertedValue, targetUnit);
        }

        public static bool operator >(Weight left, Weight right)
        {
            var normalized = right.ConvertTo(left.Unit);
            return left.Value > normalized.Value;
        }

        public static bool operator <(Weight left, Weight right)
        {
            var normalized = right.ConvertTo(left.Unit);
            return left.Value < normalized.Value;
        }

        public static bool operator >=(Weight left, Weight right) => !(left < right);
        public static bool operator <=(Weight left, Weight right) => !(left > right);

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
            yield return Unit;
        }

        public override string ToString() => $"{Value:N2} {Unit}";
    }
}
