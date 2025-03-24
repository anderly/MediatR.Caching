using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Caching.Extensions;
using Microsoft.Extensions.Logging;

namespace MediatR.Caching;

/// <summary>
/// MediatR Caching Pipeline Behavior
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class CachingAttributeBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICache _cache;
    private readonly ILogger<CachingAttributeBehavior<TRequest, TResponse>> _logger;

    public CachingAttributeBehavior(ICache cache, ILogger<CachingAttributeBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var cacheAttribute = (CacheAttribute)request.GetType().GetCustomAttributes(typeof(CacheAttribute), false).FirstOrDefault();

        if (cacheAttribute == null)
        {
            return await next();
        }

        string cacheKey;
        if (request is ICacheKey<TRequest, TResponse> req)
        {
	        cacheKey = req.GetCacheKey(request);
	        cacheKey = $"{typeof(TRequest).FullName}{{{cacheKey}}}";
		}
        else
        {
	        cacheKey = GetDefaultCacheKey(request);
        }

        var cachedResponse = await _cache.GetAsync<TResponse>(cacheKey, cancellationToken);
        if (cachedResponse != null)
        {
            _logger.LogInformation($"Response retrieved {typeof(TRequest).FullName} from cache (CacheKey: {cacheKey}).");
            return cachedResponse;
        }

        var response = await next();
        _logger.LogInformation($"Caching response for {typeof(TRequest).FullName} with CacheKey: {cacheKey}");

        await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(cacheAttribute.SlidingExpiration), null, TimeSpan.FromMinutes(cacheAttribute.AbsoluteExpiration), cancellationToken);
        return response;
    }

    static string GetDefaultCacheKey(TRequest request)
    {
	    var props = typeof(TRequest).GetProperties().Where(p => p.PropertyType.IsSimple())
		    .Select(pi => $"{pi.Name}:{pi.GetValue(request)}");
	    var cacheKey = $"{typeof(TRequest).FullName}{{{props.ToCsv()}}}";

	    return cacheKey;
    }
}