using System;
using UnityEngine.UIElements;

namespace UI.ControlElementDescriptors
{
    public class ButtonCD : IControlDescriptor
    {
        public string Label { get; }
        private readonly Action _setter;

        public ButtonCD(string label, Action setter)
        {
            Label = label;
            _setter = setter;
        }

        public VisualElement CreateElement(Action onValueChanged)
        {
            var elem = new Button();
            elem.text = Label;
            elem.clicked += () =>
            {
                _setter();
                onValueChanged?.Invoke();
            };

            return elem;
        }

        public void BindTo(VisualElement element) {}
    }
}