using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Domain.Framework;

namespace Fartak.Domain.Common
{
    public static class CommonErrors
    {
        public static Error PhoneNumberNullError()
            => Error.Create(
                code: "Phone.PhoneNumberNullError",
                message: "شماره تلفن نا معتبر است",
                details: new Dictionary<string, object>
                {
                    ["error"] = "null value"
                });

        public static Error PhoneNumberWrongFormatError()
            => Error.Create(
                code: "Phone.PhoneNumberWrongFormatError",
                message: "فرمت شماره تلفن اشتباه است",
                details: new Dictionary<string, object>
                {
                    ["error"] = "wrong format"
                });
    }
}
