using System;
using System.Collections.Generic;
using Leds.Interfaces;
using Tools;
using UI.ControlElementDescriptors;
using UnityEngine;
using Volumes;

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

        public Transform Transform { get; set; }
        
        private BaseVolume _currentBaseVolume;
        public BaseVolume CurrentBaseVolume
        {
            get => _currentBaseVolume;
            set
            {
                _currentBaseVolume = value;
                foreach (var pixel in Pixels)
                {
                    pixel.BaseVolume = _currentBaseVolume;
                }
            }
        }

        [SerializeField, SerializeProperty]
        private bool visualize;
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

            yield return new SliderCD(
                "Pixel Count",
                () => PixelCount,
                i => PixelCount = i);
        }
    }
}