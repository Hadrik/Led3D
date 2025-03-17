using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.ControlElementDescriptors
{
    public class TransformCD : IControlDescriptor
    {
        public string Label { get; }
        private readonly BoxCD _boxCD;

        public TransformCD(string label, Func<Transform> getter, Action<Vector3> setterPos, Action<Vector3> setterRot,
            Action<Vector3> setterSca)
        {
            Label = label;

            var descriptors = new List<IControlDescriptor>
            {
                new Vector3CD(
                    "Position",
                    () => getter().position,
                    value => setterPos(value)
                ),
                new Vector3CD(
                    "Rotation",
                    () => getter().rotation.eulerAngles,
                    value => setterRot(value)
                ),
                new Vector3CD(
                    "Scale",
                    () => getter().localScale,
                    value => setterSca(value)
                )
            };

            _boxCD = new BoxCD(
                label,
                descriptors,
                null,
                FlexDirection.Column
            );
        }

        public VisualElement CreateElement(Action onValueChanged)
        {
            return _boxCD.CreateElement(onValueChanged);
        }

        public void BindTo(VisualElement element)
        {
            _boxCD.BindTo(element);
        }
    }
}