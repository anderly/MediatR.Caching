using MediatR.Caching.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MediatR.Caching;

/// <summary>
/// MediatR Cache Invalidation Pipeline Behavior
/// </summary>
/// <description>
/// When injected into a MediatR pipeline, this behavior will receive a list of instances of ICacheInvalidator<TRequest>
/// instances for every class in your project that implements this interface for the given generic types.
///
/// When the request pipeline runs, the behavior will make sure the curent request runs through the pipeline and
/// after it returns will proceed to call Invalidate on any ICacheInvalidator instance in the list of invalidators. 
/// </description>
/// <typeparam name="TRequest">The type of the request that needs to invalidate other cached request responses.</typeparam>
/// <typeparam name="TResponse">The response of the request that causes invalidation of other cached request responses.</typeparam>
public class CacheInvalidationAttributeBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IMediator _mediator;
    private readonly ICache _cache;
    private readonly ILogger<CacheInvalidationAttributeBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// Create new instance of MediatR Cache Invalidation Pipeline Behavior
    /// </summary>
    /// <param name="mediator"></param>
    /// <param name="cache"></param>
    /// <param name="logger"></param>
    public CacheInvalidationAttributeBehavior(IMediator mediator, ICache cache, ILogger<CacheInvalidationAttributeBehavior<TRequest, TResponse>> logger)
    {
        _mediator = mediator;
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // run through the request handler pipeline for this request
        var result = await next();

        var invalidateCacheAttributes = request.GetType().GetCustomAttributes(typeof(InvalidateCacheAttribute), false).Select(a => (InvalidateCacheAttribute)a).ToList();

        if (!invalidateCacheAttributes.Any())
        {
            return result;
        }

        var cacheInvalidationTasks = new List<Task>();
        var cacheAutoReloadTasks = new List<Task>();
        foreach (var invalidateCacheAttribute in invalidateCacheAttributes)
        {
            var targetType = invalidateCacheAttribute.DeclaringType;
            var props = request.GetType().GetProperties().Where(p => p.PropertyType.IsSimple() && targetType.GetProperties().Where(p => p.PropertyType.IsSimple()).Select(tp => tp.Name).Contains(p.Name)).Select(pi => $"{pi.Name}:{pi.GetValue(request, null)}");
            var cacheKey = $"{targetType.FullName}{{{props.ToCsv()}}}";

            var cachedResponse = await _cache.GetAsync<TResponse>(cacheKey, cancellationToken);
            if (cachedResponse == null)
            {
                continue;
            }

            //await _cache.RemoveAsync(cacheKey, cancellationToken);
            cacheInvalidationTasks.Add(_cache.RemoveAsync(cacheKey, cancellationToken));

            var cacheAttribute = (CacheAttribute)targetType.GetCustomAttributes(typeof(CacheAttribute), false).FirstOrDefault();

            if (cacheAttribute == null) continue;

            if (!cacheAttribute.AutoReload) continue;

            var query = Activator.CreateInstance(targetType);
            request.CopyTo(query);

            //var method = _mediator.GetType().GetMethod("Send");
            var method = _mediator.GetType().GetMethods()
                .FirstOrDefault(mi => mi.Name == "Send" &&
                    mi.GetGenericArguments().Length == 1);

            // Get the IRequest<T> type
            var requestType = targetType
                .GetInterfaces()
                .First(x => x.GetGenericTypeDefinition() == typeof(IRequest<>));

            // get generic arguments of IRequest<T>
            var genericArguments = requestType.GetGenericArguments();

            // Get the T type for use in making our generic call to IMediator.Send<T>
            var firstGenericArgument = genericArguments.First();

            var generic = method?.MakeGenericMethod(firstGenericArgument);

            var task = (Task)generic?.Invoke(_mediator, new[] { query, cancellationToken });

            //if (task != null) await task;
            if (task != null)
            {
                cacheAutoReloadTasks.Add(task);
            }
        }

        if (cacheInvalidationTasks.Any())
        {
            await Task.WhenAll(cacheInvalidationTasks);
        }

        if (cacheAutoReloadTasks.Any())
        {
            await Task.WhenAll(cacheAutoReloadTasks);
        }

        return result;
    }

}