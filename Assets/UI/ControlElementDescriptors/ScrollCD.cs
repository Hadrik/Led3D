using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace UI.ControlElementDescriptors
{
    public class ScrollCD : IControlDescriptor
    {
        public string Label { get; }
        private readonly IReadOnlyList<IControlDescriptor> _descriptors;
        private readonly Dictionary<IControlDescriptor, VisualElement> _childElements = new();

        public ScrollCD(string label, IReadOnlyList<IControlDescriptor> descriptors)
        {
            Label = label;
            _descriptors = descriptors;
        }

        public VisualElement CreateElement(Action onValueChanged)
        {
            _childElements.Clear();
            
            var elem = new ScrollView();
            
            // Maybe add a label

            foreach (var descriptor in _descriptors)
            {
                var child = descriptor.CreateElement(onValueChanged);
                _childElements.Add(descriptor, child);
                elem.Add(child);
            }

            return elem;
        }

        public void BindTo(VisualElement element)
        {
            foreach (var descriptor in _descriptors)
            {
                if (_childElements.TryGetValue(descriptor, out var child))
                {
                    descriptor.BindTo(child);
                }
            }
        }
    }
}