using System;
using System.Collections.Generic;
using Tools;
using UI;
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
        
        private VolumeController _currentVolume;
        public VolumeController CurrentVolume
        {
            get => _currentVolume;
            set
            {
                _currentVolume = value;
                // foreach (var pixel in Pixels)
                // {
                //     pixel.CurrentVolume = _currentVolume;
                // }
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
    }
}