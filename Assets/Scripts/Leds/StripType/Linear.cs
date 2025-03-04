using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Leds.StripType
{
    public class Linear : BaseStrip
    {
        private Handle _startHandle;
        private Handle _endHandle;
        [SerializeField] private int pixelCount = 10;

        protected override void Start()
        {
            base.Start();
            CreateHandles();
            UpdatePixels();
        }

        protected override void Update()
        {
            base.Update();
            if (!_startHandle.transform.hasChanged && !_endHandle.transform.hasChanged) return;
            UpdatePixels();
            _startHandle.transform.hasChanged = false;
            _endHandle.transform.hasChanged = false;
        }

        private VisualElement _myUI;
        public override VisualElement GenerateUI()
        {
            var template = Resources.Load<VisualTreeAsset>("UI/StripSettingsTemplate");
            var pixelTemplate = Resources.Load<VisualTreeAsset>("UI/PixelTemplate");
            var root = template.Instantiate();

            var slider = root.Q<SliderInt>("PixelsSlider");
            var visualizeToggle = root.Q<Toggle>("Visualize");
            var list = root.Q<ListView>("PixelsList");
            
            slider.value = pixelCount;
            slider.RegisterValueChangedCallback(evt =>
            {
                pixelCount = evt.newValue;
                UpdatePixels();
            });
            
            visualizeToggle.value = Visualize;
            visualizeToggle.RegisterValueChangedCallback(evt => { Visualize = evt.newValue; });
            
            list.itemsSource = Pixels;
            list.makeItem = () => pixelTemplate.Instantiate();
            list.bindItem = (element, i) =>
            {
                element.Clear();
                var ui = Pixels[i].GenerateUI();
                element.Add(ui);
            };
            
            _myUI = root;
            return root;
        }

        private void UpdatePixels()
        {
            // changed amount
            var diff = pixelCount - Pixels.Count;
            if (diff < 0)
            {
                // remove
                for (var i = 0; i < -diff; i++)
                {
                    Pixels[pixelCount + i].Destroy();
                }
                Pixels.RemoveRange(pixelCount, -diff);
            }
            else if (diff > 0)
            {
                // add
                for (var i = 0; i < diff; i++)
                {
                    Pixels.Add(new Pixel(Pixels.Count, Vector3.zero, CurrentVolume, transform, Visualize));
                }
            }

            foreach (var pixel in Pixels)
            {
                pixel.Transform.position = Vector3.Lerp(_startHandle.transform.position, _endHandle.transform.position,
                    pixel.Index / (float)Pixels.Count);
            }
            _myUI?.Q<ListView>("PixelsList").Rebuild();
        }

        private void CreateHandles()
        {
            _startHandle = Handle.Create(transform, UpdatePixels, "StartHandle");
            _endHandle = Handle.Create(transform, UpdatePixels, "EndHandle");
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(_startHandle.transform.position, _endHandle.transform.position);
        }
    }
}