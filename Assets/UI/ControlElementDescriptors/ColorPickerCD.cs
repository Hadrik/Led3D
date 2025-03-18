using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.ControlElementDescriptors
{
    public class ColorPickerCD : IControlDescriptor
    {
        public string Label { get; }
        private readonly BoxCD _boxCD;

        public ColorPickerCD(string label, Func<Color> getter, Action<Color> setter)
        {
            Label = label;
            var color = getter();
            
            var descriptors = new List<IControlDescriptor>
            {
                new SliderCD(
                    "Hue",
                    () => HSV(color).x,
                    value =>
                    {
                        var hsv = HSV(color);
                        hsv.x = value;
                        setter(Color.HSVToRGB(hsv.x, hsv.y, hsv.z));
                        color = getter();
                    }
                ),
                new SliderCD(
                    "Saturation",
                    () => HSV(color).y,
                    value =>
                    {
                        var hsv = HSV(color);
                        hsv.y = value;
                        setter(Color.HSVToRGB(hsv.x, hsv.y, hsv.z));
                        color = getter();
                    }
                ),
                new SliderCD(
                    "Value",
                    () => HSV(color).z,
                    value =>
                    {
                        var hsv = HSV(color);
                        hsv.z = value;
                        setter(Color.HSVToRGB(hsv.x, hsv.y, hsv.z));
                        color = getter();
                    }
                ),
                new ColorCD(
                    "Preview",
                    () => color
                )
            };

            _boxCD = new BoxCD(
                "Color Picker",
                descriptors,
                null,
                FlexDirection.Column
            );
        }

        private static Vector3 HSV(Color color)
        {
            Color.RGBToHSV(color, out var h, out var s, out var v);
            return new Vector3(h, s, v);
        }

        public VisualElement CreateElement(Action onValueChanged)
        {
            return _boxCD.CreateElement(onValueChanged);
        }

        public void BindTo(VisualElement element)
        {
            _boxCD.BindTo(element);
        }
    }
}