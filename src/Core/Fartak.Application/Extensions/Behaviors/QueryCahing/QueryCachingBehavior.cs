using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fartak.Application.Contract;
using Fartak.Domain.Framework;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fartak.Application.Extensions.Behaviors.QueryCahing
{
    internal sealed class QueryCachingBehavior<TRequest, TResponse>
      : IPipelineBehavior<TRequest, TResponse>
      where TRequest : ICachedQuery
      where TResponse : Result
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<QueryCachingBehavior<TRequest, TResponse>> _logger;

        public QueryCachingBehavior(
            ICacheService cacheService,
            ILogger<QueryCachingBehavior<TRequest, TResponse>> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var type = typeof(TResponse);
            TResponse? cachedResult = await _cacheService.GetAsync<TResponse>(
                request.CacheKey,
                cancellationToken);


            if (cachedResult != null)
            {
                return cachedResult;
            }
            var result = await next();

            if (result.IsSuccess)
            {
                await _cacheService.SetAsync(request.CacheKey, result, request.Expiration, cancellationToken);
            }

            return result;
        }
    }

}
