using System.Reflection;
using NLog;

namespace Led3D_2.Utility;

// TODO: Add logging
public static class ObjectExtensions
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    
    public static Dictionary<string, object> AsDictionary(this object source,
        BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
    {
        return source.GetType().GetProperties(bindingAttr).ToDictionary
        (
            propInfo => propInfo.Name,
            propInfo => propInfo.GetValue(source, null)!
        );
    }

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

    public static PropertyInfo? GetProperty(this object source, string propertyName,
        BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
    {
        return source.GetType().GetProperty(propertyName, bindingAttr);
    }
    
    public static void SetProperty(this object source, PropertyInfo property, object? value,
        BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
    {
        try
        {
            property.SetValue(source, value, null);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to set property");
        }
    }
}