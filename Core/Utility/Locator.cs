using System.Reflection;
using NLog;

namespace Led3D_2.Utility;

public static class Locator
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    
    public static List<Type> GetClassesImplementing(Type type)
    {
        try
        {
            return Assembly.GetAssembly(type)
                !.GetTypes()
                .Where(t => type.IsAssignableFrom(t) && t.IsClass)
                .ToList();
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to get classes implementing {type}", type);
            return [];
        }
    }
}