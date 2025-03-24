using System.Numerics;
using ColorHelper;

namespace Led3D_2.Utility;

public static class MathHelpers
{
    public static bool Contains(Vector3 boxScale, Vector3 localPoint)
    {
        var halfScale = boxScale * 0.5f;
        return MathF.Abs(localPoint.X) <= halfScale.X &&
               MathF.Abs(localPoint.Y) <= halfScale.Y &&
               MathF.Abs(localPoint.Z) <= halfScale.Z;
    }
    
    public static Vector3 ToLocal(this Vector3 worldPoint, Vector3 center, Vector3 rotation)
    {
        // First subtract position (translate to origin)
        var result = worldPoint - center;
    
        // Then apply inverse rotations in reverse order (Z, Y, X)
        result = Vector3.Transform(result, Matrix4x4.CreateRotationZ(-rotation.Z));
        result = Vector3.Transform(result, Matrix4x4.CreateRotationY(-rotation.Y));
        result = Vector3.Transform(result, Matrix4x4.CreateRotationX(-rotation.X));
    
        return result;
    }
    
    public static Vector3 NormalizeByScale(this Vector3 localPoint, Vector3 scale)
    {
        // Half extents of the box
        var halfExtents = scale * 0.5f;
    
        // Shift from [-scale/2, scale/2] to [0, scale]
        var shifted = localPoint + halfExtents;
    
        // Normalize to [0, 1], handling potential division by zero
        return new Vector3(
            scale.X > float.Epsilon ? shifted.X / scale.X : 0.5f,
            scale.Y > float.Epsilon ? shifted.Y / scale.Y : 0.5f,
            scale.Z > float.Epsilon ? shifted.Z / scale.Z : 0.5f
        );
    }
    
    public static Vector3 Lerp(Vector3 start, Vector3 end, float t)
    {
        return start + (end - start) * t;
    }
}

public static class ColorHelpers
{
    public static HSV Lerp(HSV start, HSV end, float t)
    {
        // Clamp t between 0 and 1
        t = Math.Clamp(t, 0, 1);
    
        // Handle hue interpolation (considering its circular nature)
        var hueDiff = end.H - start.H;
    
        // Take the shortest path around the hue circle
        if (Math.Abs(hueDiff) > 180)
        {
            if (hueDiff > 0)
                hueDiff -= 360;
            else
                hueDiff += 360;
        }
    
        var h = start.H + hueDiff * t;
        // Normalize hue to 0-360 range
        if (h < 0) h += 360;
        if (h >= 360) h -= 360;
    
        // Linear interpolation for saturation and value
        var s = start.S + (end.S - start.S) * t;
        var v = start.V + (end.V - start.V) * t;
    
        return new HSV((int)h, (byte)s, (byte)v);
    }
}