using JetBrains.Annotations;
using Leds.UI;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Volumes;

namespace Leds
{
    public class Pixel : IUIGenerator
    {
        public Transform Transform { get; set; }
        public Color Color { get; set; }
        public int Index { get; set; }
        [CanBeNull] private Renderer Renderer { get; set; }
        [CanBeNull] private VolumeController Volume { get; set; }

        [CanBeNull] private GameObject _visualObject;
        [CanBeNull] private TemplateContainer _templateContainer;

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

        public void SetPrefab(GameObject prefab, Transform parent)
        {
            _visualObject = Object.Instantiate(prefab, parent);
            Transform = _visualObject!.transform;
            Renderer = _visualObject!.GetComponent<Renderer>();
            _visualObject.SetActive(Visualize);
        }
        
        public Pixel(int index, Vector3 position, VolumeController volume = null, Transform parent = null, bool visualize = false)
        {
            Volume = volume;
            Index = index;
            Color = Color.magenta;
            Visualize = visualize;
            
            if (parent is null) return;
            
            var prefab = Resources.Load<GameObject>("Prefabs/PixelVisualization");
            SetPrefab(prefab, parent);
            Transform.position = position;
        }

        public void Destroy()
        {
            if (_visualObject != null)
            {
                Object.Destroy(_visualObject);
            }
        }
        
        public void Update()
        {
            if (Visualize == false) return;
            if (_visualObject is null) return;
            if (Renderer is null) return;
            
            Renderer.material.color = Volume?.SampleColorAt(Transform.position) ?? Color;
            if (_templateContainer is null) return;
            _templateContainer.Q<VisualElement>("ColorPreview").style.backgroundColor = Renderer.material.color;
        }

        public VisualElement GenerateUI()
        {
            var template = Resources.Load<VisualTreeAsset>("UI/PixelTemplate");
            var root = template.Instantiate();
            _templateContainer = root;

            var index = root.Q<Label>("PixelIndex");
            var color = root.Q<VisualElement>("ColorPreview");

            index.text = $"P {Index}";
            color.style.backgroundColor = Color;
            
            return root;
            /*
            var root = new Box();

            var label = new Label($"Pixel {Index}");
            root.Add(label);

            var colorField = new ColorField("Color");
            colorField.value = Color;
            root.Add(colorField);

            return root;
            */
        }
    }
}
