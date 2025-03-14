using System;
using UnityEngine.UIElements;

namespace UI.ControlElementDescriptors
{
    public class ToggleCD : IControlDescriptor
    {
        public string Label { get; }
        private readonly Func<bool> _getter;
        private readonly Action<bool> _setter;

        public ToggleCD(string label, Func<bool> getter, Action<bool> setter)
        {
            Label = label;
            _getter = getter;
            _setter = setter;
        }

        public VisualElement CreateElement(Action onValueChanged)
        {
            var elem = new Toggle();
            elem.label = Label;
            elem.value = _getter();
            elem.RegisterValueChangedCallback(evt =>
            {
                _setter(evt.newValue);
                onValueChanged?.Invoke();
            });

            return elem;
        }

        public void BindTo(VisualElement element)
        {
            if (element is not Toggle toggle) return;

            toggle.value = _getter();
            // toggle.RegisterValueChangedCallback(evt => _setter(evt.newValue));
        }
    }
}