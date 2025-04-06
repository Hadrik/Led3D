using ColorHelper;

namespace Led3D_2.Utility.Converters;

public class HsvSettingsConverter : ISettingsConverter<HSV>
{
    public object? ToObject(HSV obj)
    {
        return new
        {
            h = obj.H,
            s = obj.S,
            v = obj.V,
        };
    }

    public HSV FromObject(object obj)
    {
        if (obj is not Dictionary<string, object> dict)
            throw new ArgumentException("Object is not a dictionary.");

        if (!dict.TryGetValue("h", out var hObj) || !dict.TryGetValue("s", out var sObj) || !dict.TryGetValue("v", out var vObj))
            throw new ArgumentException("Dictionary does not contain h, s, or v.");

        return new HSV(Convert.ToInt32(hObj), Convert.ToByte(sObj), Convert.ToByte(vObj));
    }
}