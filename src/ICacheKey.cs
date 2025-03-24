using System.Linq;
using MediatR.Caching.Extensions;

namespace MediatR.Caching
{
	public interface ICacheKey<in TRequest, TResponse> where TRequest : IRequest<TResponse>
	{
		string GetCacheKey(TRequest request)
		{
			var r = new { request };
			var props = r.request.GetType().GetProperties().Where(p => p.PropertyType.IsSimple()).Select(pi => $"{pi.Name}:{pi.GetValue(r.request, null)}");
			return $"{typeof(TRequest).FullName}{{{props.ToCsv()}}}";
		}
}
}
