using System;
using UnityEngine.UIElements;

namespace UI.ControlElementDescriptors
{
    public class FoldoutCD : IControlDescriptor
    {
        public string Label { get; }
        private readonly IControlDescriptor _childDescriptor;
        private VisualElement _childElement;

        public FoldoutCD(string label, IControlDescriptor childDescriptor)
        {
            Label = label;
            _childDescriptor = childDescriptor;
        }

        public VisualElement CreateElement(Action onValueChanged)
        {
            var elem = new Foldout();
            elem.text = Label;
            
            _childElement = _childDescriptor.CreateElement(onValueChanged);
            elem.Add(_childElement);

            return elem;
        }

        public void BindTo(VisualElement element)
        {
            _childDescriptor.BindTo(_childElement);
        }
    }
}