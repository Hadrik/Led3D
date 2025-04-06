using System.Numerics;

namespace Led3D_2.Utility.Converters;

public class Vector3SettingsConverter : ISettingsConverter<Vector3>
{
    public object? ToObject(Vector3 obj)
    {
        return new
        {
            x = obj.X,
            y = obj.Y,
            z = obj.Z
        };
    }

    public Vector3 FromObject(object obj)
    {
        if (obj is not Dictionary<string, object> dict)
            throw new ArgumentException("Object is not a dictionary.");

        if (!dict.TryGetValue("x", out var xObj) || !dict.TryGetValue("y", out var yObj) || !dict.TryGetValue("z", out var zObj))
            throw new ArgumentException("Dictionary does not contain x, y, or z.");

        return new Vector3(Convert.ToSingle(xObj), Convert.ToSingle(yObj), Convert.ToSingle(zObj));
    }
}