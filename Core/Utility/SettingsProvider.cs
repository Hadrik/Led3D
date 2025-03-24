using System.Linq.Expressions;
using System.Reflection;
using NLog;

namespace Led3D_2.Utility;

public interface ISettingsProvider
{
    Dictionary<string, string> GetAvailableSettings();
    Dictionary<string, object> GetSettings();
    void UpdateSettings(Dictionary<string, object> newSettings);
    void UpdateSetting(string key, object newValue);
}

public class SettingsProvider<T>(T settings) : ISettingsProvider
    where T : class, new()
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    
    private readonly Dictionary<PropertyInfo, Delegate> _changeHandlers = new();
    
    /// <summary>
    /// Get types of all settings
    /// </summary>
    /// <returns>
    /// Dictionary (setting name, setting type)
    /// </returns>
    public Dictionary<string, string> GetAvailableSettings() => settings.AsTypeDictionary();
    
    /// <summary>
    /// Get values of all settings
    /// </summary>
    /// <returns>
    /// Dictionary (setting name, setting value)
    /// </returns>
    public Dictionary<string, object> GetSettings() => settings.AsDictionary();
    
    /// <summary>
    /// Update settings with new values
    /// </summary>
    /// <param name="newSettings">
    /// Dictionary (setting name, new setting value)
    /// </param>
    public void UpdateSettings(Dictionary<string, object> newSettings)
    {
        foreach (var (key, value) in newSettings)
        {
            UpdateSetting(key, value);
        }
    }
    
    /// <summary>
    /// Update a single setting with a new value
    /// </summary>
    /// <param name="key">
    /// Setting name to update
    /// </param>
    /// <param name="newValue">
    /// New setting value
    /// </param>
    public void UpdateSetting(string key, object newValue)
    {
        var property = settings.GetProperty(key);
        if (!Verifier.VerifyProperty(property, key, newValue)) return;
        
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
    
    private static string GetPropertyName<TProp>(Expression<Func<T, TProp>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }
        Log.Error("Expression is not a member access - {name}", nameof(expression));
        return "";
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
}