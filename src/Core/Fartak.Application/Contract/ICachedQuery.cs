using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Fartak.Application.Contract
{
    public interface ICachedQuery<TResponse> : IRequest<TResponse>
    {
        string CacheKey { get; }
        TimeSpan? Expiration { get; }
    }
}
