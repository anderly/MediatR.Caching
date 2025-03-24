using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using MediatR.Caching.Extensions;

namespace MediatR.Caching;

/// <summary>
/// MediatR Caching Pipeline Behavior
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
public class CacheInvalidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IMediator _mediator;
    private readonly ICache _cache;
    private readonly ILogger<CacheInvalidationBehavior<TRequest, TResponse>> _logger;
    private readonly IEnumerable<ICacheInvalidationPolicy<TRequest>> _cacheInvalidationPolicies;

    public CacheInvalidationBehavior(IMediator mediator,
        ICache cache,
        ILogger<CacheInvalidationBehavior<TRequest, TResponse>> logger,
        IEnumerable<ICacheInvalidationPolicy<TRequest>> cacheInvalidationPolicies
    )
    {
        _mediator = mediator;
        _cache = cache;
        _logger = logger;
        _cacheInvalidationPolicies = cacheInvalidationPolicies;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // run through the request handler pipeline for this request
        var result = await next();

        if (!_cacheInvalidationPolicies.Any())
        {
            return result;
        }

        // now loop through each cache invalidator for this request type and call the Invalidate method passing
        // an instance of this request in order to retrieve a cache key.
        var cacheInvalidationTasks = new List<Task>();
        var cacheAutoReloadTasks = new List<Task>();

        foreach (var cacheInvalidationPolicy in _cacheInvalidationPolicies)
        {
            var cacheKey = $"{cacheInvalidationPolicy.GetCacheKey(request)}";
            //await _cache.RemoveAsync(cacheKey, cancellationToken);
            cacheInvalidationTasks.Add(_cache.RemoveAsync(cacheKey, cancellationToken));

            var cachePolicy = cacheInvalidationPolicy.GetCachePolicy();

            if (cachePolicy == null) continue;

            if (!cachePolicy.AutoReload) continue;

            var targetType = cacheInvalidationPolicy.GetTargetType();

            var query = Activator.CreateInstance(targetType);
            request.CopyTo(query);

            var method = _mediator.GetType().GetMethod("Send");

            // Get the IRequest<T> type
            var requestType = targetType
                .GetInterfaces()
                .First(x => x.GetGenericTypeDefinition() == typeof(IRequest<>));

            // get generic arguments of IRequest<T>
            var genericArguments = requestType.GetGenericArguments();

            // Get the T type for use in making our generic call to IMediator.Send<T>
            var firstGenericArgument = genericArguments.First();

            var generic = method?.MakeGenericMethod(firstGenericArgument);

            var task = (Task)generic?.Invoke(_mediator, [query, cancellationToken]);

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