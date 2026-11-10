using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Domain.Framework
{
    public abstract class DomainException : Exception
    {
        public string Code { get; }
        public Dictionary<string, object>? Details { get; }

        protected DomainException(string code, string message, Dictionary<string, object>? details = null)
            : base(message)
        {
            Code = code;
            Details = details;
        }
    }
}
