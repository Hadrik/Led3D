using System;
using System.Collections.Generic;
using Leds.Interfaces;
using Tools;
using UI.ControlElementDescriptors;
using UnityEngine;
using Volumes;
using Volumes.Interfaces;

namespace Leds
{
    public abstract class BaseStrip : MonoBehaviour, IStrip
    {
        public event Action NameChanged;
        public event Action VisualizationChanged;
        public abstract event Action<int> PixelCountChanged;

        public string Name
        {
            get => name;
            set
            {
                name = value;
                NameChanged?.Invoke();
            }
        }


        protected List<Pixel> _pixels = new();
        public IReadOnlyList<Pixel> Pixels => _pixels.AsReadOnly();
        public abstract int PixelCount { get; set; }

        private IVolume _currentVolume;

        public IVolume CurrentVolume
        {
            get => _currentVolume;
            set
            {
                _currentVolume = value;
                foreach (var pixel in Pixels)
                {
                    pixel.Volume = _currentVolume;
                }
            }
        }

        [SerializeField, SerializeProperty] private bool visualize;

        public bool Visualize
        {
            get => visualize;
            set
            {
                visualize = value;
                foreach (var pixel in _pixels)
                {
                    pixel.Visualize = visualize;
                }

                VisualizationChanged?.Invoke();
            }
        }


        public void SetPixelPrefab(GameObject prefab)
        {
            foreach (var pixel in _pixels)
            {
                pixel.SetPrefab(prefab, transform);
            }
        }

        protected virtual void Update()
        {
            foreach (var pixel in _pixels)
            {
                pixel.Update();
            }
        }

        public virtual void Destroy()
        {
            foreach (var pixel in _pixels)
            {
                pixel.Destroy();
            }

            Destroy(gameObject);
        }

        public virtual IEnumerable<IControlDescriptor> GetUIControls()
        {
            yield return new ToggleCD(
                "Visualize",
                () => Visualize,
                b => Visualize = b);

            yield return new VolumeDropdownCD(
                "Volume",
                () => CurrentVolume,
                (v) => CurrentVolume = v
            );

            yield return new SliderCD(
                "Pixel Count",
                () => PixelCount,
                i => PixelCount = i,
                0,
                100
            );
        }
    }
}