namespace MediatR.Caching.Extensions
{
	internal static class ObjectExtensions
	{
		/// <summary>
		/// Copies matching property names and types to the passed destination object.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		public static void CopyTo(this object source, object destination)
		{
			var sourceProperties = source.GetType().GetProperties();
			var destinationProperties = destination.GetType().GetProperties();

			foreach (var sourceProperty in sourceProperties)
			{
				foreach (var destinationProperty in destinationProperties)
				{
					if (sourceProperty.Name == destinationProperty.Name && sourceProperty.PropertyType == destinationProperty.PropertyType)
					{
						destinationProperty.SetValue(destination, sourceProperty.GetValue(source));
						break;
					}
				}
			}
		}

		public static object GetPropertyValue(object obj, string propertyName)
		{
			var objType = obj.GetType();
			var prop = objType.GetProperty(propertyName);

			return prop.GetValue(obj, null);
		}
	}
}
