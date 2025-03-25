using System.Reflection;
using NLog;

namespace Led3D_2.Utility;

public static class ObjectExtensions
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    
    /// <summary>
    /// Transform an object into a dictionary
    /// </summary>
    /// <param name="source">
    /// Object to transform
    /// </param>
    /// <param name="bindingAttr">
    /// BindingFlags to use when getting properties
    /// </param>
    /// <returns>
    /// Dictionary (property name, property value)
    /// </returns>
    public static Dictionary<string, object> AsDictionary(this object source,
        BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
    {
        return source.GetType().GetProperties(bindingAttr).ToDictionary
        (
            propInfo => propInfo.Name,
            propInfo => propInfo.GetValue(source, null)!
        );
    }

    /// <summary>
    /// Transform an object into a dictionary of property names and their types
    /// </summary>
    /// <param name="source">
    /// Object to transform
    /// </param>
    /// <param name="bindingAttr">
    /// BindingFlags to use when getting properties
    /// </param>
    /// <returns>
    /// Dictionary (property name, property type).
    /// If property type is an interface, the value will be a string with all classes implementing it separated by ';'
    /// </returns>
    public static Dictionary<string, string> AsTypeDictionary(this object source,
        BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
    {
        var result = new Dictionary<string, string>();
        var properties = source.GetType().GetProperties(bindingAttr);
        
        // For each property in class
        foreach (var propInfo in properties)
        {
            var propertyType = propInfo.PropertyType;

            // If property is an interface, get all classes implementing it
            if (propertyType.IsInterface)
            {
                var implementations = Locator.GetClassesImplementing(propertyType);
                var implementationNames = string.Join(';', implementations.Select(t => t.Name));
                result[propInfo.Name] = implementationNames;
            }
            // Else just add the type name
            else
            {
                result[propInfo.Name] = propertyType.Name;
            }
        }
        
        return result;
    }

    /// <summary>
    /// Get a property by name
    /// </summary>
    /// <param name="source">
    /// Object to get property from
    /// </param>
    /// <param name="propertyName">
    /// Name of the property to get
    /// </param>
    /// <param name="bindingAttr">
    /// BindingFlags to use when getting the property
    /// </param>
    /// <returns>
    /// The property with the given name or null if not found
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// propertyName is null
    /// </exception>
    public static PropertyInfo? GetProperty(this object source, string propertyName,
        BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
    {
        return source.GetType().GetProperty(propertyName, bindingAttr);
    }
    
    /// <summary>
    /// Set a property by name
    /// </summary>
    /// <param name="source">
    /// Object to set property on
    /// </param>
    /// <param name="property">
    /// Property to set
    /// </param>
    /// <param name="value">
    /// Value to set the property to
    /// </param>
    /// <exception cref="Exception">
    /// All <c>PropertyInfo.SetValue</c> exceptions
    /// </exception>
    public static void SetProperty(this object source, PropertyInfo property, object? value)
    {
        try
        {
            property.SetValue(source, value, null);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to set property");
            throw;
        }
    }
}