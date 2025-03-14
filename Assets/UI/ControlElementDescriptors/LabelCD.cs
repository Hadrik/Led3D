using System;
using UnityEngine.UIElements;

namespace UI.ControlElementDescriptors
{
    public class LabelCD : IControlDescriptor
    {
        public string Label { get; }

        public LabelCD(string label)
        {
            Label = label;
        }

        public VisualElement CreateElement(Action onValueChanged)
        {
            var elem = new Label();
            elem.text = Label;

            return elem;
        }

        public void BindTo(VisualElement element) {}
    }
}