using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.ControlElementDescriptors
{
    public class BoxCD : IControlDescriptor
    {
        public string Label { get; }
        private readonly IReadOnlyList<IControlDescriptor> _descriptors;
        private readonly Dictionary<IControlDescriptor, VisualElement> _childElements = new();

        private readonly int[] _flexGrow;
        private readonly FlexDirection _flexDirection;

        public BoxCD(string label, IReadOnlyList<IControlDescriptor> children, int[] flexGrow = null, FlexDirection direction = FlexDirection.Row)
        {
            Label = label;
            _descriptors = children;
            _flexGrow = flexGrow;
            _flexDirection = direction;
            if (flexGrow != null && children.Count != flexGrow.Length)
            {
                Debug.LogWarning($"Box content descriptor '{label}' has {children.Count} children but {flexGrow.Length} flex grow values");
            }
        }

        public VisualElement CreateElement(Action onValueChanged)
        {
            var elem = new VisualElement();
            elem.style.flexDirection = _flexDirection;

            for (var i = 0; i < _descriptors.Count; i++)
            {
                var descriptor = _descriptors[i];
                var child = descriptor.CreateElement(onValueChanged);
                child.style.flexGrow = _flexGrow?.Length > i ? _flexGrow[i] : 1;
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