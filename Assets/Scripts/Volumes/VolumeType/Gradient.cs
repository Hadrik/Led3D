using UnityEngine;

namespace Volumes.VolumeType
{
    public class Gradient : BaseVolume
    {
        public Color StartColor = Color.red;
        public Color EndColor = Color.blue;
        public Vector3 Direction = Vector3.right;
        
        public override Color SampleColorAt(Vector3 position)
        {
            var localPos = WorldToLocal(position);
            
            if (!Contains(localPos))
            {
                return Color.black;
            }

            var normalizedPos = Normalized(position);
            
            // Get position along gradient direction (normalized to 0-1)
            var t = Vector3.Dot(normalizedPos - Vector3.one * 0.5f, Direction) + 0.5f;
            t = Mathf.Clamp01(t);
            
            // Interpolate color
            return Color.Lerp(StartColor, EndColor, t);
        }
    }
}
