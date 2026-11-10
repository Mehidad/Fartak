using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Common
{
    public sealed class Phone : ValueObject
    {
        public string Value { get; }

        private Phone() { } // برای EF

        private Phone(string phone)
        {
            Value = phone;
        }

        public static Result<Phone> Create(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return Result.Failure<Phone>(CommonErrors.PhoneNumberNullError());

            if (!phone.All(char.IsDigit))
                return Result.Failure<Phone>(CommonErrors.PhoneNumberWrongFormatError());

            if (!phone.StartsWith("09") && !phone.StartsWith("98") && !phone.StartsWith("+98"))
                return Result.Failure<Phone>(CommonErrors.PhoneNumberWrongFormatError());

            if ((phone.StartsWith("09") && phone.Length != 11) ||
                (phone.StartsWith("98") && phone[2] != '9' && phone.Length != 12) ||
                (phone.StartsWith("+98") && phone[3] != '9' && phone.Length != 13)
                )
                return Result.Failure<Phone>(CommonErrors.PhoneNumberWrongFormatError());

            string NormalizedPhone = Normalize(phone);
            if (!IsValid(NormalizedPhone))
                return Result.Failure<Phone>(CommonErrors.PhoneNumberWrongFormatError());

            return Result<Phone>.Success(new Phone(NormalizedPhone));
        }

        public static string Normalize(string phone)
        {
            phone = phone.Replace(" ", "");
            if (phone.StartsWith("09"))
                return "98" + phone.Substring(1);
            if (phone.StartsWith("+98"))
                return phone.Substring(1);
            return phone;
        }

        public string ToLocal(string normalPhone) => "0" + normalPhone.Substring(2);

        public static bool IsValid(string phone) =>
            phone.Length == 12 && phone.StartsWith("989") && phone.All(char.IsDigit);

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
