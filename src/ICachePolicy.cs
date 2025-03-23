using System;
using System.Linq;
using MediatR.Caching.Extensions;

namespace MediatR.Caching;

public interface ICachePolicy<TRequest, TResponse>
{
    DateTime? AbsoluteExpiration => null;
    TimeSpan? AbsoluteExpirationRelativeToNow => TimeSpan.FromMinutes(5);
    TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(1);

    bool AutoReload => false;

    //string GetCacheKeyPrefix() => typeof(TResponse).FullName;

    string GetCacheKey(TRequest request)
    {
        var r = new {request};
        var props = r.request.GetType().GetProperties().Where(p => p.PropertyType.IsSimple()).Select(pi => $"{pi.Name}:{pi.GetValue(r.request, null)}");
        return $"{typeof(TRequest).FullName}{{{props.ToCsv()}}}";
        //return $"{{{props.ToCsv()}}}";
    }


}