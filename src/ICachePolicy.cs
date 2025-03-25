using System;
using System.Linq;
using MediatR.Caching.Extensions;

namespace MediatR.Caching;

public interface ICachePolicy<in TRequest, TResponse>
	: ICacheKey<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    DateTime? AbsoluteExpiration { get; set; }// => null;
    TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }// => TimeSpan.FromMinutes(5);
	TimeSpan? SlidingExpiration { get; set; }// => TimeSpan.FromMinutes(1);

	bool AutoReload { get; set; }// => false;

    //string GetCacheKeyPrefix() => typeof(TResponse).FullName;
}