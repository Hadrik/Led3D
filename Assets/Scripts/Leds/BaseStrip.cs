using System;
using System.Collections.Generic;
using System.Linq;
using Global;
using Tools;
using UnityEngine;
using UnityEngine.UIElements;
using Volumes;

namespace Leds
{
    public abstract class BaseStrip : MonoBehaviour, IStrip
    {
        public string Name
        {
            get => name;
            set => name = value;
        }
        
        public List<Pixel> Pixels { get; protected set; } = new();

        public Transform Transform
        {
            get => transform;
            set => transform.position = value.position;
        }
        
        public List<VolumeController> Volumes { get; set; }
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
                foreach (var pixel in Pixels)
                {
                    pixel.Visualize = visualize;
                }
            }
        }


        protected virtual void Start()
        {
            var g = GameObject.FindWithTag("GameController");
            if (g is null)
            {
                Debug.LogError("GameController not found");
                return;
            }
            Volumes = g.GetComponent<GameController>().volumeControllers;
            if (Volumes.Count > 0)
            {
                CurrentVolume = Volumes[0];
            }
        }

        public void SetPixelPrefab(GameObject prefab)
        {
            foreach (var pixel in Pixels)
            {
                pixel.SetPrefab(prefab, transform);
            }
        }
        
        protected virtual void Update()
        {
            foreach (var pixel in Pixels)
            {
                pixel.Update();
            }
        }

        protected List<VisualElement> GetPixelUI()
        {
            return Pixels.Select(pixel => pixel.GenerateUI()).ToList();
        }

        public abstract VisualElement GenerateUI();
    }
}