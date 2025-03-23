using System;

namespace MediatR.Caching;

/// <summary>
/// Custom MediatR attribute that, when applied on a Request/Query <see cref="IRequest{TResponse}" />, 
/// causes the invalidation of the passed DeclaringType <see cref="IRequest{TResponse}" />.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class InvalidateCacheAttribute : Attribute
{
    public Type DeclaringType { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidateCacheAttribute" /> class.
    /// </summary>
    /// <param name="declaringType">The type of MediatR request/query to invalidate when this request is run.</param>
    public InvalidateCacheAttribute(Type declaringType)
    {
        DeclaringType = declaringType;
    }
}