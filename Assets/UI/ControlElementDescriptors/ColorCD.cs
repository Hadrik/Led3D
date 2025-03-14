using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.ControlElementDescriptors
{
    public class ColorCD : IControlDescriptor
    {
        public string Label { get; }
        private readonly Func<Color> _getter;

        public ColorCD(string label, Func<Color> getter)
        {
            Label = label;
            _getter = getter;
        }

        public VisualElement CreateElement(Action onValueChanged)
        {
            var elem = new VisualElement();
            elem.style.backgroundColor = _getter();

            return elem;
        }

        public void BindTo(VisualElement element)
        {
            element.style.backgroundColor = _getter();
        }
    }
}