using System;
using System.Reflection;

namespace MediatR.Caching.Extensions
{
	internal static class TypeExtensions
	{

		public static bool IsSimple(this Type type)
		{
			var typeInfo = type.GetTypeInfo();
			if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				// nullable type, check if the nested type is simple.
				return typeInfo.GetGenericArguments()[0].IsSimple();
			}
			return typeInfo.IsPrimitive
			       || typeInfo.IsEnum
			       || type == typeof(string)
			       || type == typeof(decimal);
		}

	}
}
