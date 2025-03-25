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
    /// Get types of all settings
    /// </summary>
    /// <returns>
    /// <code>
    /// [
    ///     {
    ///         Name: "settingName",
    ///         Type: "settingType",
    ///         Subsettings: [
    ///             {...}
    ///         ]
    ///     }, ...
    /// ]
    /// </code>
    /// </returns>
    List<IDictionary<string, object?>> GetAvailableSettings();
    
    /// <summary>
    /// Get values of all settings
    /// </summary>
    /// <returns>
    /// Dictionary (setting name, setting value)
    /// </returns>
    Dictionary<string, object> GetSettingValues();
    
    /// <summary>
    /// Update settings with new values
    /// </summary>
    /// <param name="newSettings">
    /// Dictionary (setting name, new setting value)
    /// </param>
    void UpdateSettings(Dictionary<string, object> newSettings);
    
    /// <summary>
    /// Update a single setting with a new value
    /// </summary>
    /// <param name="key">
    /// Setting name to update
    /// </param>
    /// <param name="newValue">
    /// New setting value
    /// </param>
    /// <exception cref="NullReferenceException">
    /// property not found
    /// </exception>
    void UpdateSetting(string key, object newValue);
}


public class Setting<T>
{
    public string Name { get; init; } = string.Empty;
    public string? FriendlyName { get; init; }
    public string? Description { get; init; }
    
    private T _value;
    public T Value
    {
        get => _value;
        set
        {
            if (Validate != null && !Validate(value))
            {
                throw new ArgumentException("Invalid value");
            }
            _value = value;
        }
    }
    public T DefaultValue { init => _value = value; }
    public Type Type => typeof(T);
    public Predicate<T>? Validate;
}


/// <summary>
/// Helper class to manage settings of a class
/// </summary>
/// <param name="settings">
/// Instance of the settings class
/// </param>
/// <typeparam name="T">
/// Class implementing the settings.
/// All properties of the class will be considered a setting
/// </typeparam>
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

    public List<IDictionary<string, object?>> GetAvailableSettings()
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
            repr.Type = setting.Type.Name;

            if (typeof(ISettingsProvider).IsAssignableFrom(setting.Type))
            {
                repr.Value = setting.Value?.GetAvailableSettings();
            }
            else
            {
                repr.Value = setting.Value;
            }
                
            result.Add((IDictionary<string, object?>)repr);
        }
        return result;
    }

    public Dictionary<string, object> GetSettingValues()
    {
        var result = new Dictionary<string, object>();
        foreach (var property in GetSettingProperties())
        {
            dynamic? setting = property.GetValue(this);
            if (setting != null)
            {
                result[setting.Name] = setting.Value;
            }
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
            if (setting != null && setting.Name == key)
            {
                var oldValue = setting.Value;
                var valueType = setting.Type;
                if (valueType.IsInterface)
                {
                    if (newValue is string)
                    {
                        var objInstance = InstantiateImplementation(valueType, (string)newValue);
                        if (objInstance != null)
                        {
                            setting.Value = objInstance;
                        }
                        else
                        {
                            Log.Warn("Failed to instantiate implementation {name} for interface {interface}", 
                                (string)newValue, valueType.Name);
                            throw new ArgumentException($"Failed to instantiate implementation {(string)newValue}");
                        }
                    }
                    else
                    {
                        Log.Warn("Setting is an interface, but new value is not a string");
                        throw new ArgumentException("Setting is an interface, but new value is not a string");
                    }
                }
                else
                {
                    setting.Value = newValue;
                }

                if (_changeHandlers.TryGetValue(setting.Name, out Delegate? handlerDelegate))
                {
                    handlerDelegate?.DynamicInvoke(oldValue, setting.Value);
                }
                
                return;
            }
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
