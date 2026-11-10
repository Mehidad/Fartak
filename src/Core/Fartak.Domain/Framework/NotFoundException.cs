using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fartak.Domain.Framework
{
    public class NotFoundException : DomainException
    {
        public NotFoundException(string entityName, object id)
            : base(
                code: "NOT_FOUND",
                message: $"{entityName} با شناسه {id} یافت نشد",
                details: new Dictionary<string, object>
                {
                    ["EntityName"] = entityName,
                    ["Id"] = id
                })
        {
        }
    }
}
