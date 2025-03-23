using System.Collections.Generic;

namespace MediatR.Caching.Extensions;

internal static class IEnumerableStringExtensions
{
    /// <summary>
    /// Helper extension method to return a delimited string from a <see cref="List{T}(string)"/>
    /// </summary>
    /// <param name="list"></param>
    /// <param name="delimiter"></param>
    /// <returns></returns>
    public static string ToDelimited(this IEnumerable<string> list, string delimiter)
    {
        return string.Join(delimiter, list);
    }

    /// <summary>
    /// Helper extension method to return a comma-separated list from a <see cref="List{T}(string)"/>
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public static string ToCsv(this IEnumerable<string> list)
    {
        return list.ToDelimited(",");
    }
}