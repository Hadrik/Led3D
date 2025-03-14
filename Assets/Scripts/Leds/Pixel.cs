using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Leds.Interfaces;
using UI.ControlElementDescriptors;
using UnityEngine;
using Volumes;
using Object = UnityEngine.Object;

namespace Leds
{
    public class Pixel : IPixel
    {
        public Vector3 Position { get; set; }
        public Color Color { get; private set; }
        public int Index { get; set; }
        [CanBeNull] private Renderer Renderer { get; set; }
        [CanBeNull] public BaseVolume BaseVolume { get; set; }

        [CanBeNull] private GameObject _visualObject;

        private bool _visualize;
        public bool Visualize
        {
            get => _visualize;
            set
            {
                _visualize = value;
                _visualObject?.SetActive(value);
            }
        }

        public event Action<Color> ColorChanged;

        public void SetPrefab(GameObject prefab, Transform parent)
        {
            _visualObject = Object.Instantiate(prefab, parent);
            Renderer = _visualObject!.GetComponent<Renderer>();
            _visualObject.SetActive(Visualize);
        }
        
        public Pixel(int index, BaseVolume baseVolume = null, Transform parent = null, bool visualize = false)
        {
            BaseVolume = baseVolume;
            Index = index;
            Color = Color.magenta;
            Visualize = visualize;
            
            if (parent is null) return;
            
            var prefab = Resources.Load<GameObject>("Prefabs/PixelVisualization");
            SetPrefab(prefab, parent);
        }

        public void Destroy()
        {
            if (_visualObject is not null)
            {
                Object.Destroy(_visualObject);
            }
        }
        
        public void Update()
        {
            var color = BaseVolume?.SampleColorAt(Position) ?? Color;
            if (color == Color) return;
            
            Color = color;
            ColorChanged?.Invoke(Color);
            
            if (Renderer is null || _visualObject is null || _visualize == false) return;
            Renderer.material.color = Color;
        }
        
        public IEnumerable<IControlDescriptor> GetUIControls()
        {
            yield return new BoxCD(
                "Pixel",
                new IControlDescriptor[]{
                    new LabelCD(Index.ToString()), // TODO: breaks on name change
                    new ColorCD("Color", () => Color)
                },
                new[] { 1, 1 }
                );
        }
    }
}
