using System;
using System.Collections.Generic;
using Leds.Interfaces;
using Tools;
using UnityEngine;
using Linear = Leds.StripType.Linear;

namespace Leds
{
    public class Driver : MonoBehaviour, IDriver
    {
        private List<BaseStrip> _strips = new();
        public IReadOnlyList<IStrip> Strips => _strips.AsReadOnly();
        public string Name => name;

        [SerializeField, SerializeProperty] private bool visualize;
        public bool Visualize
        {
            get => visualize;
            set
            {
                if (value == visualize) return;
                visualize = value;
                foreach (var strip in _strips)
                {
                    strip.Visualize = visualize;
                }
                VisualizationChanged?.Invoke();
            }
        }

        public event Action<IStrip> StripAdded;
        public event Action<IStrip> StripRemoved;
        public event Action VisualizationChanged;


        [SerializeField, SerializeProperty] private GameObject visualisationPrefab;
        private GameObject VisualisationPrefab
        {
            get => visualisationPrefab;
            set
            {
                visualisationPrefab = value;
                foreach (var strip in _strips)
                {
                    strip.SetPixelPrefab(visualisationPrefab);
                }
            }
        }

        private void Start()
        {
            var pf = Resources.Load<GameObject>("Prefabs/PixelVisualization");
            if (pf is null)
            {
                Debug.LogError("Prefabs/PixelVisualization could not be loaded");
            }
            else
            {
                VisualisationPrefab = pf;
            }
        }
        
        public void Destroy()
        {
            foreach (var strip in _strips)
            {
                strip.Destroy();
            }
            _strips.Clear();
            Destroy(gameObject);
        }

        public void AddStrip<T>() where T : IStrip
        {
            var pf = Resources.Load<GameObject>("Prefabs/Strip");
            var strip = Instantiate(pf, transform);
            strip.name = $"Strip_{_strips.Count}";

            var component = strip.AddComponent<Linear>();
            if (VisualisationPrefab is not null)
            {
                component.SetPixelPrefab(visualisationPrefab);
            }

            _strips.Add(component);
            StripAdded?.Invoke(component);
        }

        public void RemoveStrip(IStrip strip)
        {
            var found = _strips.Find(s => ReferenceEquals(s, strip));
            if (found is null)
            {
                Debug.LogError($"Driver {name} tried to remove non-existing strip {strip.Name}");
                return;
            }
            found.Destroy();
            _strips.Remove(found);
            StripRemoved?.Invoke(found);
        }
    }
}