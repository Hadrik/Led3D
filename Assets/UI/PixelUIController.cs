using Leds.Interfaces;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public static class PixelUIController
    {
        private static readonly VisualTreeAsset PixelAsset = Resources.Load<VisualTreeAsset>("UI/PixelTemplate");
        
        public static VisualElement Create(IPixel pixel)
        {
            var instance = PixelAsset.Instantiate();
            
            var index = instance.Q<Label>("PixelIndex");
            var color = instance.Q<VisualElement>("ColorPreview");
            
            color.style.backgroundColor = pixel.Color;
            pixel.ColorChanged += (newColor) => { color.style.backgroundColor = newColor; };
            
            index.text = pixel.Index.ToString();

            return instance;
        }
    }
}