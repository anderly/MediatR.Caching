# MediatR.Caching

MediatR.Caching is a DotNetCore library that provides support for caching [MediatR](https://github.com/jbogard/MediatR) `IRequest<T>` responses. It also includes support for invalidating cache results using MediatR commands.

## Features

- Cache responses of MediatR `IRequest<T>` queries using any supported [IDistributedCache](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed) provider.
- Invalidate cached responses using MediatR commands.
- Easy integration with existing MediatR setup.
- Configurable caching strategies.

## Installation

You can install the package via NuGet:

```sh
dotnet add package MediatR.Caching
```

## Usage

### Caching Requests

Assuming the following model and request:

```csharp
public record Model
{
    public int Id { get; init; }
    public string Title { get; init; }
    public int Credits { get; init; }
    [Display(Name = "Department")]
    public string DepartmentName { get; init; }
}

public record Query : IRequest<Model>
{
    public int? Id { get; init; }
}
```

To cache MediatR request responses, you can either use attributes like so:

```csharp
[Cache(SlidingExpiration = 1, AbsoluteExpiration = 5)]
public record Query : IRequest<Model>
{
    public int? Id { get; init; }
}
```

Or, define a CachePolicy for your request like so:

```csharp
// This could be in the same file as your handler or in a separate file.
public class CachePolicy : ICachePolicy<Query, Model>
{
    // Optionally, change defaults
    public TimeSpan? AbsoluteExpirationRelativeToNow => TimeSpan.FromMinutes(10);
    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(1);

    // Optionally, provide a different implementation here. By default the CacheKey will be in the following format:
    //     Query{Id:123}
    public string GetCacheKey(Query query)
    {
        return $"Your-Custom-Cache-Key";
    }
}
```

### Invalidating Cache

To invalidate a cached response, you can mark your commands with the InvalidateCache attribute like so:

```csharp
[InvalidateCache(typeof(Query))]
public class Command : IRequest
{
    [Display(Name = "Number")]
    public int Id { get; init; }
    public string Title { get; init; }
    public int? Credits { get; init; }
    public Department Department { get; init; }
}
```

Or, define a CacheInvalidationPolicy like so:

```csharp
public class CacheInvalidationPolicy(IEnumerable<ICachePolicy<CachingWithPoliciesModel.Query, CachingWithPoliciesModel.Result>> cachePolicies)
	        : AbstractCacheInvalidationPolicy<Query, CachingWithPoliciesModel.Query, CachingWithPoliciesModel.Result>(cachePolicies);
```

### Configuration

Register the caching behavior in your `Startup.cs` or wherever you configure MediatR:

```csharp
services.AddMediatR(typeof(Startup));
services.AddMediatRCaching();
```

Ensure you also have an IDistributedCache registered or a default MemoryCache will be registered.

```csharp

// MemoryCache (this is the default that will get registered if none provided)
services.AddDistributedMemoryCache();

// Sql Server Cache
services.AddDistributedSqlServerCache(options =>
{
    options.ConnectionString =
        Configuration.GetConnectionString("Cache");
    options.SchemaName = "dbo";
    options.TableName = "Cache";
});

// Redis Cache
services.AddStackExchangeRedisCache(options =>
{
	options.Configuration = "localhost";
	options.InstanceName = "SampleInstance";
});
```

## License

This project is licensed under the MIT License.


