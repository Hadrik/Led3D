using System.Dynamic;
using System.Reflection;
using NLog;

namespace Led3D_2.Utility;

// TODO: Fix outdated comments

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


public class Setting<T> : Setting<T, T>;
public class Setting<T, TOverride>
{
    public required string Name { get; init; }
    public string? FriendlyName { get; init; }
    public string? Description { get; init; }
    public bool ReadOnly { get; init; } = false;
    public T Value { get; set; }
    public Type BaseType => typeof(T);
    public Type OverrideType => typeof(TOverride);
    public Action<TOverride>? Setter { get; init; }
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
            p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Setting<>)
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
            repr.Type = setting.OverrideType.Name;
            
            if (typeof(ISettingsProvider).IsAssignableFrom(setting.Value.GetType()))
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
            
            // Does the setting define a type override?
            if (setting.OverrideType != setting.BaseType)
            {
                UpdateCustomSetting(setting, newValue);
            }
            // Or is it an interface?
            else if (setting.Type.IsInterface)
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
        else if (typeof(ISettingsProvider).IsAssignableFrom(setting.BaseType))
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
        var objInstance = InstantiateImplementation(setting.BaseType, implementationName);
        if (objInstance != null)
        {
            setting.Value = objInstance;
        }
        else
        {
            Log.Warn("Failed to instantiate implementation {name} for interface {interface}", 
                implementationName, setting.BaseType.Name);
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
        try
        {
            setting.Value = Convert.ChangeType(newValue, setting.BaseType);
        }
        catch (InvalidCastException ex)
        {
            Log.Warn(ex, "Failed to convert {value} to {type}", newValue, setting.BaseType.Name);
            throw new ArgumentException($"Failed to convert {newValue} to {setting.BaseType.Name}");
        }
    }

    private static void UpdateCustomSetting(dynamic setting, object newValue)
    {
        try
        {
            setting.Setter(newValue);
        }
        catch (Exception ex)
        {
            Log.Warn(ex, "Failed to set {name} to {value}", setting.Name, newValue);
            throw new InvalidOperationException($"Failed to set {setting.Name} to {newValue}");
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
