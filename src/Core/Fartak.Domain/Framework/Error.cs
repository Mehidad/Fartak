using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Domain.Framework
{
    public sealed record Error
    {
        public string Code { get; }
        public string Message { get; }
        public Dictionary<string, object>? Details { get; }

        private Error(string code, string message, Dictionary<string, object>? details = null)
        {
            Code = code;
            Message = message;
            Details = details;
        }

        public static Error Create(string code, string message, Dictionary<string, object>? details = null)
            => new(code, message, details);

        public static Error Validation(string code, string message, params (string key, object value)[] details)
            => new(code, message, details?.ToDictionary(x => x.key, x => x.value));

        public static Error NotFound(string entityName, object id)
            => new("NOT_FOUND", $"{entityName} با شناسه {id} یافت نشد",
                new Dictionary<string, object> { ["EntityName"] = entityName, ["Id"] = id });

        public static readonly Error None = new(string.Empty, string.Empty);

        public static readonly Error NullValue = new("Error.NullValue", "Null value was provided");

    }
    //public record Error(string Code, string Name)
    //{
    //    public static Error None = new(string.Empty, string.Empty);

    //    public static Error NullValue = new("Error.NullValue", "Null value was provided");
    //}

}
