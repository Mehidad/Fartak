using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Application.Framework.Queries;

namespace Fartak.Application.Extensions.Behaviors.QueryCahing
{
    public interface ICachedQuery<TResponse> : IQuery<TResponse>, ICachedQuery;

    public interface ICachedQuery
    {
        string CacheKey { get; }

        TimeSpan? Expiration { get; }
    }
}
