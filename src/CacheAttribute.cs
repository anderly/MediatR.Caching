using System;

namespace MediatR.Caching;

/// <summary>
/// Custom MediatR attribute that, when applied on a Query <see cref="IRequest{TResponse}" />, causes the return value of the
/// query to be cached for the specific list of properties (arguments) in the query.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class CacheAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the total duration, in minutes, during which the result of the current
    /// method is stored in cache. (default = 5 minutes)
    /// </summary>
    public double AbsoluteExpiration { get; set; } = 5;

    /// <summary>
    /// Gets or sets the duration, in minutes, during which the result of the current
    ///  method is stored in cache after it has been added to or accessed from the cache.
    ///  The expiration is extended every time the value is accessed from the cache. (default = 1 minute)
    /// </summary>
    public double SlidingExpiration { get; set; } = 1;

    /// <summary>
    /// Gets or sets whether the method calls are automatically reloaded by re-evaluating
    /// the target method with the same arguments when the cache item is removed from
    /// the cache. (default = false)
    /// </summary>
    public bool AutoReload { get; set; }

}