using MediatR.Caching.Extensions;
using System;
using System.Linq;

namespace MediatR.Caching;

public interface ICacheableRequest<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    DateTime? AbsoluteExpiration => null;
    TimeSpan? AbsoluteExpirationRelativeToNow => null;
    TimeSpan? SlidingExpiration => null;

    string GetCacheKey(TRequest request)
    {
        var r = new {request};
        var props = r.request.GetType().GetProperties().Select(pi => $"{pi.Name}:{pi.GetValue(r.request, null)}");
        return $"{typeof(TRequest).FullName}{{{props.ToCsv()}}}";
    }
}