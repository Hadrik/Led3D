using System.Linq.Expressions;
using System.Reflection;
using NLog;

namespace Led3D_2.Utility;

/// <summary>
/// Any class that stores settings accessible from the API has to implement this interface
/// </summary>
public interface ISettingsProvider
{
    /// <summary>
    /// Get types of all settings
    /// </summary>
    /// <returns>
    /// Dictionary (setting name, setting type)
    /// </returns>
    Dictionary<string, string> GetAvailableSettings();
    
    /// <summary>
    /// Get values of all settings
    /// </summary>
    /// <returns>
    /// Dictionary (setting name, setting value)
    /// </returns>
    Dictionary<string, object> GetSettings();
    
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
    /// <exception cref="IncorrectTypeException">
    /// newValue has incorrect type
    /// </exception>
    void UpdateSetting(string key, object newValue);
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
public class SettingsProvider<T>(T settings) : ISettingsProvider
    where T : class, new()
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    
    private readonly Dictionary<PropertyInfo, Delegate> _changeHandlers = new();
    
    
    public Dictionary<string, string> GetAvailableSettings() => settings.AsTypeDictionary();
    
    public Dictionary<string, object> GetSettings() => settings.AsDictionary();
    
    public void UpdateSettings(Dictionary<string, object> newSettings)
    {
        foreach (var (key, value) in newSettings)
        {
            UpdateSetting(key, value);
        }
    }
    
    public void UpdateSetting(string key, object newValue)
    {
        var property = settings.GetProperty(key);
        
        // Throws if property not found or incorrect type
        VerifyProperty(property, key, newValue);
        
        var oldValue = property!.GetValue(settings);

        // Is interface?
        if (property.PropertyType.IsInterface && newValue is string implementationName)
        {
            var instance = InstantiateImplementation(property.PropertyType, implementationName);
            settings.SetProperty(property, instance);
        }
        else
        {
            settings.SetProperty(property, newValue);
        }

        if (_changeHandlers.TryGetValue(property, out var handlerDelegate))
        {
            handlerDelegate.DynamicInvoke(oldValue, property.GetValue(settings));
        }
    }
    
    /// <summary>
    /// Register a callback that gets called when the property changes
    /// </summary>
    /// <param name="propertySelector">
    /// Property to register the change handler for (s => s.Property)
    /// </param>
    /// <param name="handler">
    /// Action to call when the property changes (oldValue, newValue)
    /// </param>
    /// <exception cref="ArgumentException">
    /// Invalid expression
    /// </exception>
    public void RegisterChangeHandler<TProp>(Expression<Func<T, TProp>> propertySelector, Action<TProp, TProp> handler)
    {
        var propertyName = GetPropertyName(propertySelector);
        var property = settings.GetProperty(propertyName);
        if (property == null)
        {
            Log.Warn("Failed to register change handler for {name}: Property not found", propertyName);
            return;
        }
        _changeHandlers[property] = handler;
    }
    
    /// <summary>
    /// Get the name of the property from the expression
    /// </summary>
    /// <param name="expression">
    /// (s => s.Property)
    /// </param>
    /// <typeparam name="TProp">
    /// Type of the property
    /// </typeparam>
    /// <returns>
    /// Property name
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Invalid expression
    /// </exception>
    private static string GetPropertyName<TProp>(Expression<Func<T, TProp>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }
        Log.Error("Expression is not a member access - {name}", nameof(expression));
        throw new ArgumentException("Expression is not a member access - {name}", nameof(expression));
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
    /// <returns></returns>
    private object? InstantiateImplementation(Type interfaceType, string implementationName)
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
    
    /// <summary>
    /// Verify that the property exists and the value has the correct type
    /// </summary>
    /// <param name="prop">
    /// Property to verify
    /// </param>
    /// <param name="key">
    /// Property name (for logging)
    /// </param>
    /// <param name="value">
    /// Value to verify
    /// </param>
    /// <exception cref="NullReferenceException">
    /// Property not found
    /// </exception>
    /// <exception cref="IncorrectTypeException">
    /// Incorrect property type
    /// </exception>
    private static void VerifyProperty(PropertyInfo? prop, string key, object value)
    {
        if (prop is null)
        {
            Log.Warn("Tried to update unknown setting {0}", key);
            throw new NullReferenceException("Property not found");
        }
        if (prop.GetType() != value.GetType())
        {
            Log.Warn($"Tried to update '{key}' with value of type '{value.GetType().Name}' instead of '{prop.GetType().Name}'");
            throw new IncorrectTypeException("Incorrect property type");
        }
    }
    
}
public class IncorrectTypeException(string message) : Exception(message);
