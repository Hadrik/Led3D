using System;
using UnityEngine;

namespace Leds.StripType
{
    public class Linear : BaseStrip
    {
        private Handle _startHandle;
        private Handle _endHandle;
        
        public override event Action<int> PixelCountChanged;
        public override int PixelCount
        {
            get => _pixels.Count;
            set
            {
                if (value == _pixels.Count) return;
                UpdatePixelCount(value);
                PixelCountChanged?.Invoke(value);
            }
        }

        protected void Start()
        {
            CreateHandles();
            UpdatePixelCount(10);
        }

        public override void Destroy()
        {
            Destroy(_startHandle);
            Destroy(_endHandle);
            base.Destroy();
        }

        private void UpdatePixelCount(int targetCount)
        {
            // changed amount
            var diff = targetCount - _pixels.Count;
            if (diff < 0)
            {
                // remove
                for (var i = 0; i < -diff; i++)
                {
                    _pixels[targetCount + i].Destroy();
                }
                _pixels.RemoveRange(targetCount, -diff);
            }
            else if (diff > 0)
            {
                // add
                for (var i = 0; i < diff; i++)
                {
                    _pixels.Add(new Pixel(_pixels.Count, CurrentBaseVolume, transform, Visualize));
                }
            }
            
            UpdatePixelPositions();
        }

        private void UpdatePixelPositions()
        {
            foreach (var pixel in _pixels)
            {
                pixel.Position = Vector3.Lerp(_startHandle.transform.position, _endHandle.transform.position,
                    pixel.Index / (float)_pixels.Count);
            }
        }

        private void CreateHandles()
        {
            _startHandle = Handle.Create(transform, UpdatePixelPositions, "StartHandle");
            _endHandle = Handle.Create(transform, UpdatePixelPositions, "EndHandle");
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(_startHandle.transform.position, _endHandle.transform.position);
        }
    }
}