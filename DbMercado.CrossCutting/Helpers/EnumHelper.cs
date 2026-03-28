using System.Reflection;
using System.Xml.Serialization;

namespace DbMercado.CrossCutting.Helpers;


public static class EnumHelper
{
	/// <summary>
	/// http://www.wackylabs.net/2006/06/getting-the-xmlenumattribute-value-for-an-enum-field/
	/// </summary>
	/// <param name="e"></param>
	/// <returns></returns>
	public static string? GetStringFromEnum(Enum e)
	{
        ArgumentNullException.ThrowIfNull(e, nameof(e));

        // Get the Type of the enum
        Type t = e.GetType();
        if (!t.IsEnum) throw new ArgumentException("Provided value is not an enum type.", nameof(e));

		// Get enum name
		var enumName = e.ToString("G");
        if (string.IsNullOrEmpty(enumName)) throw new ArgumentException("Enum value cannot be null or empty.", nameof(e));

        // Get the FieldInfo for the member field with the enums name
        FieldInfo? info = t.GetField(enumName);
        if (info == null) throw new ArgumentException($"Enum value '{enumName}' does not exist in enum type '{t.Name}'.", nameof(e));

        // Check to see if the XmlEnumAttribute is defined on this field
        if (!info.IsDefined(typeof(XmlEnumAttribute), false))
		{
			// If no XmlEnumAttribute then return the string version of the enum.
			return e.ToString("G");
		}

		// Get the XmlEnumAttribute
		object[] o = info.GetCustomAttributes(typeof(XmlEnumAttribute), false);
		XmlEnumAttribute att = (XmlEnumAttribute)o[0];

		return att.Name?? null;
	}


	/// <summary>
	/// Transforma as entradas de um Enum em um IEnumerable.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns>IEnumerable do ENum fornecido no tipo do método.</returns>
	/// 
	public static IEnumerable<T> GetEnumIEnumerable<T>()
	{
		return Enum.GetValues(typeof(T)).Cast<T>();
	}
}
