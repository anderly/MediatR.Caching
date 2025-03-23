using System;
using System.Collections.Generic;
using System.Linq;
using MediatR.Caching.Extensions;

namespace MediatR.Caching;

public interface ICacheInvalidationPolicy<TRequest>
{
    string GetCacheKeyPrefix();

    Type GetTargetType();

    string GetCacheKey(TRequest request)
    {
        var r = new { request };
        var props = r.request.GetType().GetProperties().Where(p => p.PropertyType.IsSimple()).Select(pi => $"{pi.Name}:{pi.GetValue(r.request, null)}");
        //return $"{typeof(TRequest).FullName}{{{props.ToCsv()}}}";
        return $"{{{props.ToCsv()}}}";
    }

    dynamic GetCachePolicy();
}

public abstract class AbstractCacheInvalidationPolicy<TRequest, TCachedRequest, TCachedResponse> : ICacheInvalidationPolicy<TRequest>
{
    private readonly IEnumerable<ICachePolicy<TCachedRequest, TCachedResponse>> _cachePolicies;

    public AbstractCacheInvalidationPolicy(IEnumerable<ICachePolicy<TCachedRequest, TCachedResponse>> cachePolicies)
    {
        _cachePolicies = cachePolicies;
    }
    public virtual string GetCacheKeyPrefix() => typeof(TCachedResponse).FullName;

    public virtual string GetCacheKey(TRequest request)
    {
        var targetType = GetTargetType();
        var props = request.GetType().GetProperties().Where(p => p.PropertyType.IsSimple() && targetType.GetProperties().Where(p => p.PropertyType.IsSimple()).Select(tp => tp.Name).Contains(p.Name)).Select(pi => $"{pi.Name}:{pi.GetValue(request, null)}");
        var cacheKey = $"{targetType.FullName}{{{props.ToCsv()}}}";

        return cacheKey;
    }

    public Type GetTargetType()
    {
        return typeof(TCachedRequest);
    }

    public dynamic GetCachePolicy()
    {
        return _cachePolicies.FirstOrDefault();
    }
    //{
    //	var r = new { request };
    //	var props = request.GetType().GetProperties().Where(p => p.PropertyType.IsSimple()).Select(pi => $"{pi.Name}:{pi.GetValue(request, null)}");
    //	//var cacheKey = $"{cachedRequestName}{{{props.ToCsv()}}}";
    //	return $"{{{String.Join(",", props)}}}";
    //}
}