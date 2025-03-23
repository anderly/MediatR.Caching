using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace MediatR.Caching.Extensions
{
	/// <summary>
	/// The IDistributedCache interface is designed to work solely with byte arrays, unlike IMemoryCache which can
	/// take any arbitrary object.
	///
	/// Microsoft has indicated that they do not intend adding these extension methods to support automatically
	/// serializing objects in several github issue e.g. https://github.com/aspnet/Caching/pull/94
	///
	/// </summary>
	internal static class DistributedCacheExtensions
	{
		public static async Task SetAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options, CancellationToken token = default)
		{
			byte[] bytes;
			await using (var memoryStream = new MemoryStream())
			{
				await JsonSerializer.SerializeAsync<T>(memoryStream, value, options: null, token);
				bytes = memoryStream.ToArray();
			}

			await cache.SetAsync(key, bytes, options, token);
		}

		public static async Task<T> GetAsync<T>(this IDistributedCache cache, string key, CancellationToken token = default)
		{
			var val = await cache.GetAsync(key, token);
			var result = default(T);

			if (val == null) return result;

			await using (var memoryStream = new MemoryStream(val))
			{
				result = await JsonSerializer.DeserializeAsync<T>(memoryStream, options: null, token);
			}

			return result;
		}
	}
}
