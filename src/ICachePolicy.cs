using System;
using System.Linq;
using MediatR.Caching.Extensions;

namespace MediatR.Caching;

public interface ICachePolicy<in TRequest, TResponse>
	: ICacheKey<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    DateTime? AbsoluteExpiration => null;
    TimeSpan? AbsoluteExpirationRelativeToNow => TimeSpan.FromMinutes(5);
    TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(1);

    bool AutoReload => false;

    //string GetCacheKeyPrefix() => typeof(TResponse).FullName;
}