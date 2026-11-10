using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Domain.Framework
{
    public class DomainValidationException : DomainException
    {
        public DomainValidationException(string code, string message, Dictionary<string, object>? details = null)
            : base(code, message, details)
        {
        }

        public static DomainValidationException Create(string code, string message, params (string key, object value)[] details)
        {
            var detailsDict = details?.ToDictionary(x => x.key, x => x.value);
            return new DomainValidationException(code, message, detailsDict);
        }
    }
}
