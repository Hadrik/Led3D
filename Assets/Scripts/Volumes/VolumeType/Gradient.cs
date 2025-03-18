using System.Collections.Generic;
using UI.ControlElementDescriptors;
using UnityEngine;

namespace Volumes.VolumeType
{
    public class Gradient : BaseVolume
    {
        private Color _startColor = Color.red;
        private Color _endColor = Color.blue;
        private Vector3 _direction = Vector3.right;
        
        public override Color SampleColorAt(Vector3 position)
        {
            var localPos = WorldToLocal(position);
            
            if (!Contains(localPos))
            {
                return Color.black;
            }

            var normalizedPos = Normalized(position);
            
            // Get position along gradient direction (normalized to 0-1)
            var t = Vector3.Dot(normalizedPos - Vector3.one * 0.5f, _direction) + 0.5f;
            t = Mathf.Clamp01(t);
            
            // Interpolate color
            return Color.Lerp(_startColor, _endColor, t);
        }

        public override IEnumerable<IControlDescriptor> GetUIControls()
        {
            foreach (var controlDescriptor in base.GetUIControls()) yield return controlDescriptor;

            yield return new ColorPickerCD(
                "StartColor",
                () => _startColor,
                c => _startColor = c
                );
            
            yield return new ColorPickerCD(
                "EndColor",
                () => _endColor,
                c => _endColor = c
            );
            
            yield return new Vector3CD(
                "Direction",
                () => _direction,
                v => _direction = v
            );
            
            yield return new TransformCD(
                "Bounds",
                () => transform,
                (p) => transform.position = p,
                (r) => transform.rotation = Quaternion.Euler(r),
                (s) => transform.localScale = s
            );
        }
    }
}
