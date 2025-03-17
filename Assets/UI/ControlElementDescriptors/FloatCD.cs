using System;
using UnityEngine.UIElements;

namespace UI.ControlElementDescriptors
{
    public class FloatCD : IControlDescriptor
    {
        public string Label { get; }
        private readonly Func<float> _getter;
        private readonly Action<float> _setter;
        
        public FloatCD(string label, Func<float> getter, Action<float> setter)
        {
            Label = label;
            _getter = getter;
            _setter = setter;
        }
        
        public VisualElement CreateElement(Action onValueChanged)
        {
            var elem = new FloatField(Label);
            
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
            if (element is not FloatField floatField) return;

            floatField.value = _getter();
        }
    }
}