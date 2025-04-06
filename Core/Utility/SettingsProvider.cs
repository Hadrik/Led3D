using System.Dynamic;
using System.Reflection;
using NLog;

namespace Led3D_2.Utility;

/// <summary>
/// Any class that stores settings accessible from the API has to implement this interface
/// </summary>
public interface ISettingsProvider
{
    /// <summary>
    /// Get all settings
    /// </summary>
    /// <returns>
    /// <code>
    /// [
    ///     { // Setting object
    ///         Name: string,
    ///         FriendlyName: string,
    ///         Description: string,
    ///         Type: Type,
    ///         Value: value of any type || subsettings
    ///     }, ...
    /// ]
    /// </code>
    /// </returns>
    List<IDictionary<string, object?>> GetSettings();
    
    /// <summary>
    /// Update settings with new values
    /// </summary>
    /// <param name="newSettings">
    /// Dictionary representation of JSON object
    /// <code>
    /// {
    ///     "settingName": newValue,
    ///     "settingName": {
    ///         "subsettingName": newValue
    ///     }
    /// }
    /// </code>
    /// </param>
    void UpdateSettings(Dictionary<string, object> newSettings);
    
    /// <summary>
    /// Update a single setting with a new value
    /// </summary>
    /// <param name="key">
    /// Setting name to update
    /// </param>
    /// <param name="newValue">
    /// Value of any type or subsetting object
    /// </param>
    /// <exception cref="ArgumentException">
    /// Incorrect value type or subsetting object
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// Setting with the specified name not found
    /// </exception>
    /// <exception cref="NullReferenceException">
    /// Modifying subsettings of a null object
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Setting is read-only
    /// </exception>
    void UpdateSetting(string key, object newValue);
}

public interface ISettingsConverter<T>
{
    /// <summary>
    /// Return an object representation of the setting
    /// </summary>
    /// <param name="obj">
    /// Object to convert
    /// </param>
    object? ToObject(T obj);
    
    /// <summary>
    /// Create a new setting value from an object
    /// </summary>
    /// <param name="obj">
    /// Object to convert
    /// </param>
    /// <returns></returns>
    T FromObject(object obj);
}

public class Setting<T>
{
    /// <summary>
    /// Name for the option used in the API
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Name for the option displayed in the UI
    /// </summary>
    public string? FriendlyName { get; init; }
    
    /// <summary>
    /// Option description
    /// </summary>
    public string? Description { get; init; }
    public bool ReadOnly { get; init; } = false;
    public T Value { get; set; } = default!;
    
    /// <summary>
    /// Type of the stored value
    /// </summary>
    public Type Type => typeof(T);
    
    /// <summary>
    /// Custom converter for the setting
    /// </summary>
    public ISettingsConverter<T>? Converter { get; init; }
    
    /// <summary>
    /// Function that returns a list of options for the setting.
    /// By default, if the option type is an interface it will return a list of all implementations, otherwise null.
    /// </summary>
    public Func<IList<string>>? Options { get; init; }

    
    /// <summary>
    /// Wrapper function for the converter. Dynamics don't work with generics
    /// </summary>
    public void ConverterFromObject(object newValue)
    {
        if (Converter != null)
        {
            Value = Converter.FromObject(newValue);
        }
    }
    /// <inheritdoc cref="ConverterFromObject"/>
    public object? ConverterToObject()
    {
        return Converter?.ToObject(Value);
    }
}


/// <summary>
/// Helper to manage settings of a class
/// </summary>
public class SettingsProvider : ISettingsProvider
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    
    private readonly Dictionary<string, Delegate> _changeHandlers = new();

    private IEnumerable<PropertyInfo> GetSettingProperties()
    {
        return GetType().GetProperties().Where(p => 
            p.PropertyType.IsGenericType &&
            p.PropertyType.GetGenericTypeDefinition() == typeof(Setting<>)
        );
    }

    public List<IDictionary<string, object?>> GetSettings()
    {
        var result = new List<IDictionary<string, object?>>();
        foreach (var property in GetSettingProperties())
        {
            dynamic? setting = property.GetValue(this);
            if (setting == null) continue;
            
            dynamic repr = new ExpandoObject();
            repr.Name = setting.Name;
            repr.FriendlyName = setting.FriendlyName;
            repr.Description = setting.Description;
            repr.ReadOnly = setting.ReadOnly;
            repr.Type = setting.Type.Name;
            
            if (setting.Options != null)
            {
                repr.Options = setting.Options();
            }
            else if (setting.Type.IsInterface)
            {
                List<Type> impls = Locator.GetClassesImplementing(setting.Type);
                repr.Options = impls.Select(t => t.Name).ToList();
            }
            else
            {
                repr.Options = null;
            }
            
            if (setting.Converter != null)
            {
                repr.Value = setting.ConverterToObject();
            }
            else if (typeof(ISettingsProvider).IsAssignableFrom(setting.Type))
            {
                repr.Value = setting.Value?.GetSettings();
            }
            else
            {
                repr.Value = setting.Value;
            }
                
            result.Add((IDictionary<string, object?>)repr);
        }
        return result;
    }
    
    public void UpdateSettings(Dictionary<string, object> newSettings)
    {
        foreach (var (key, value) in newSettings)
        {
            UpdateSetting(key, value);
        }
    }
    
    public void UpdateSetting(string key, object newValue)
    {
        foreach (var property in GetSettingProperties())
        {
            dynamic? setting = property.GetValue(this);
            if (setting == null || setting!.Name != key) continue;

            var oldValue = setting!.Value;
            
            // Does the setting define converter?
            if (setting.Converter != null)
            {
                setting.ConverterFromObject(newValue);
            }
            // Is it an interface or does it have subsettings?
            else if (setting.Type.IsInterface || typeof(ISettingsProvider).IsAssignableFrom(setting.Type))
            {
                UpdateInterfaceSetting(setting, newValue);
            }
            // If neither just try to set it directly
            else
            {
                UpdatePrimitiveSetting(setting, newValue);
            }

            InvokeChangeHandler(setting.Name, oldValue, setting.Value);
            return;
        }
        
        Log.Warn("Setting '{name}' not found", key);
        throw new KeyNotFoundException($"Setting '{key}' not found");
    }
    
    private static void UpdateInterfaceSetting(dynamic setting, object newValue)
    {
        if (newValue is string strVal)
        {
            UpdateInterfaceFromString(setting, strVal);
        }
        else if (typeof(ISettingsProvider).IsAssignableFrom(setting.Type))
        {
            UpdateSubSettings(setting, newValue);
        }
        else
        {
            Log.Warn("Setting is an interface without subsettings, but new value is not a string");
            throw new ArgumentException("Setting is an interface without subsettings, but new value is not a string");
        }
    }

    private static void UpdateInterfaceFromString(dynamic setting, string implementationName)
    {
        var objInstance = InstantiateImplementation(setting.Type, implementationName);
        if (objInstance != null)
        {
            setting.Value = objInstance;
        }
        else
        {
            Log.Warn("Failed to instantiate implementation {name} for interface {interface}", 
                implementationName, setting.Type.Name);
            throw new ArgumentException($"Failed to instantiate implementation {implementationName}");
        }
    }
    
    private static void UpdateSubSettings(dynamic setting, object newValue)
    {
        if (setting.Value == null)
        {
            Log.Warn("Trying to update subsettings of null object");
            throw new NullReferenceException("Cannot update subsettings of null object");
        }
        
        if (newValue is Dictionary<string, object?> dictVal)
        {
            setting.Value.UpdateSettings(dictVal);
        }
        else
        {
            Log.Warn("Trying to update subsettings with a non-dictionary object");
            throw new ArgumentException("Cannot update subsettings with a non-dictionary object");
        }
    }
    
    private static void UpdatePrimitiveSetting(dynamic setting, object newValue)
    {
        if (setting.ReadOnly)
        {
            Log.Warn("Setting {name} is read-only", setting.Name);
            throw new InvalidOperationException($"Setting {setting.Name} is read-only");
        }
        try
        {
            setting.Value = Convert.ChangeType(newValue, setting.Type);
        }
        catch (InvalidCastException ex)
        {
            Log.Warn(ex, "Failed to convert {value} to {type}", newValue, setting.Type.Name);
            throw new ArgumentException($"Failed to convert {newValue} to {setting.Type.Name}");
        }
    }
    
    private void InvokeChangeHandler(string settingName, object oldValue, object newValue)
    {
        if (_changeHandlers.TryGetValue(settingName, out var handlerDelegate))
        {
            handlerDelegate.DynamicInvoke(oldValue, newValue);
        }
    }
    
    public void RegisterChangeHandler<T>(Setting<T> setting, Action<T, T> handler)
    {
        try
        {
            _changeHandlers[setting.Name] = handler;
        }
        catch (KeyNotFoundException ex)
        {
            Log.Warn(ex, "Failed to register change handler for {name}", setting.Name);
        }
    }
    
    /// <summary>
    /// Create a new instance of a class implementing the interface
    /// </summary>
    /// <param name="interfaceType">
    /// Type of the interface to instantiate
    /// </param>
    /// <param name="implementationName">
    /// Name of the class implementing the interface
    /// </param>
    /// <returns>
    /// Instance of the specified class
    /// </returns>
    private static object? InstantiateImplementation(Type interfaceType, string implementationName)
    {
        try
        {
            var implementations = Locator.GetClassesImplementing(interfaceType);
            var implementation = implementations.FirstOrDefault(t => t.Name == implementationName);
            
            if (implementation == null)
            {
                Log.Error("Implementation {name} not found for interface {interface}", 
                    implementationName, interfaceType.Name);
                return null;
            }
            
            return Activator.CreateInstance(implementation);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to instantiate {name}", implementationName);
            return null;
        }
    }
}
