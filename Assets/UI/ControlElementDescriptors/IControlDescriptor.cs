using System;
using JetBrains.Annotations;
using UnityEngine.UIElements;

namespace UI.ControlElementDescriptors
{
    public interface IControlDescriptor
    {
        string Label { get; }
        VisualElement CreateElement([CanBeNull] Action onValueChanged);
        void BindTo(VisualElement element);
    }
}