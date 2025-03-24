using System.Reflection;
using NLog;

namespace Led3D_2.Utility;

public static class Verifier
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    
    public static bool VerifyProperty(PropertyInfo? prop, string key, object value)
    {
        if (prop is null)
        {
            Log.Warn("Tried to update unknown setting {0}", key);
            return false;
        }
        if (prop.GetType() != value.GetType())
        {
            Log.Warn($"Tried to update '{key}' with value of type '{value.GetType().Name}' instead of '{prop.GetType().Name}'");
            return false;
        }

        return true;
    }
}