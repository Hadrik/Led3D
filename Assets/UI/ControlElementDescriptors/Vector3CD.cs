using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.ControlElementDescriptors
{
    public class Vector3CD : IControlDescriptor
    {
        public string Label { get; }
        private readonly Func<Vector3> _getter;
        private readonly Action<Vector3> _setter;
        
        public Vector3CD(string label, Func<Vector3> getter, Action<Vector3> setter)
        {
            Label = label;
            _getter = getter;
            _setter = setter;
        }
        
        public VisualElement CreateElement(Action onValueChanged)
        {
            var elem = new Vector3Field(Label);
            elem.style.flexDirection = FlexDirection.Column;
            
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
            if (element is not Vector3Field floatField) return;

            floatField.value = _getter();
        }
    }
}