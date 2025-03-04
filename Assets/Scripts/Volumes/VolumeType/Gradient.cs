using UnityEngine;

namespace Volumes.VolumeType
{
    public class Gradient : IVolume
    {
        private Vector3 _origin;
        private Vector3 _size;
        private Bounds _bounds;
        
        public Color StartColor = Color.red;
        public Color EndColor = Color.blue;
        public Vector3 Direction = Vector3.right;
        
        public Gradient(Vector3 origin, Vector3 size)
        {
            _origin = origin;
            _size = size;
            _bounds = new Bounds(origin, size);
        }

        public bool Contains(Vector3 position)
        {
            return _bounds.Contains(position);
        }
        
        public Color SampleColorAt(Vector3 position)
        {
            if (!Contains(position))
            {
                return Color.black;
            }
            
            // Convert world position to local position
            Vector3 localPos = position - (_origin - _size / 2);
            
            // Convert to normalized position (0-1 range)
            Vector3 normalizedPos = new Vector3(
                Mathf.Clamp01(localPos.x / _size.x),
                Mathf.Clamp01(localPos.y / _size.y),
                Mathf.Clamp01(localPos.z / _size.z)
            );
            
            // Get position along gradient direction (normalized to 0-1)
            float t = Vector3.Dot(normalizedPos - Vector3.one * 0.5f, Direction) + 0.5f;
            t = Mathf.Clamp01(t);
            
            // Interpolate color
            return Color.Lerp(StartColor, EndColor, t);
        }
    }
}
