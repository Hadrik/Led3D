﻿using System;
using UnityEngine.UIElements;

namespace UI.ControlElementDescriptors
{
    public class SliderCD : IControlDescriptor
    {
        public string Label { get; }
        private readonly Func<float> _getterF;
        private readonly Func<int> _getter;
        private readonly Action<float> _setterF;
        private readonly Action<int> _setter;
        private readonly bool _isFloat;

        public SliderCD(string label, Func<float> getter, Action<float> setter)
        {
            Label = label;
            _getterF = getter;
            _setterF = setter;
            _isFloat = true;
        }
        public SliderCD(string label, Func<int> getter, Action<int> setter)
        {
            Label = label;
            _getter = getter;
            _setter = setter;
            _isFloat = false;
        }

        public VisualElement CreateElement(Action onValueChanged)
        {
            // Ew
            if (_isFloat)
            {
                var elem = new Slider();
                elem.label = Label;
                elem.value = _getterF();
                elem.RegisterValueChangedCallback(evt =>
                {
                    _setterF(evt.newValue);
                    onValueChanged?.Invoke();
                });
                return elem;
            }
            else
            {
                var elem = new SliderInt();
                elem.label = Label;
                elem.value = _getter();
                elem.RegisterValueChangedCallback(evt =>
                {
                    _setter(evt.newValue);
                    onValueChanged?.Invoke();
                });
                return elem;
            }
        }

        public void BindTo(VisualElement element)
        {
            if (element is not Slider toggle) return;

            toggle.value = _getter();
        }
    }
}