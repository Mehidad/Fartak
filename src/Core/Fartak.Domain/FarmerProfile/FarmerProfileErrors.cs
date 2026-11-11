using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.FarmerProfile
{
    public static class FarmerProfileErrors
    {
        public static Error NullReferenceError()
            => Error.Create(
                code: "FarmerProfile.NullReference",
                message: "مقادیر اجباری پروفایل کشاورز ارسال نشده است.");

        public static Error InvalidFixedPhone(string? fixedPhone)
            => Error.Create(
                code: "FarmerProfile.InvalidFixedPhone",
                message: "تلفن ثابت باید فقط رقم و بین ۸ تا ۱۵ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["FixedPhone"] = fixedPhone ?? string.Empty
                });

        public static Error VillageTooLong(string village)
            => Error.Create(
                code: "FarmerProfile.VillageTooLong",
                message: "نام روستا نباید بیش از ۱۲۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["Village"] = village
                });

        public static Error BankNameTooLong(string bankName)
            => Error.Create(
                code: "FarmerProfile.BankNameTooLong",
                message: "نام بانک نباید بیش از ۱۰۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["BankName"] = bankName
                });

        public static Error InvalidAccountNo(string? accountNo)
            => Error.Create(
                code: "FarmerProfile.InvalidAccountNo",
                message: "شماره حساب باید فقط رقم و حداکثر ۳۰ کاراکتر باشد.",
                details: new Dictionary<string, object>
                {
                    ["AccountNo"] = accountNo ?? string.Empty
                });
    }
}
