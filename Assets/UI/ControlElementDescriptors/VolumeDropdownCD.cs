using System;
using System.Collections.Generic;
using System.Linq;
using Global;
using UnityEngine;
using UnityEngine.UIElements;
using Volumes.Interfaces;

namespace UI.ControlElementDescriptors
{
    public class VolumeDropdownCD : IControlDescriptor
    {
        public string Label { get; }
        private readonly Func<IVolume> _getter;
        private readonly Action<IVolume> _setter;

        private static readonly AppController AppController =
            GameObject.FindWithTag("GameController").GetComponent<AppController>();

        public VolumeDropdownCD(string label, Func<IVolume> getter, Action<IVolume> setter)
        {
            Label = label;
            _getter = getter;
            _setter = setter;
        }

        public VisualElement CreateElement(Action onValueChanged)
        {
            var current = _getter()?.Name;
            var nameList = new List<string>();
            if (current is null)
            {
                nameList.Add("None");
            }
            nameList.AddRange(AppController.Volumes.ToList().ConvertAll(v => v.Name));
            
            var elem = new DropdownField(Label, nameList, 0);

            elem.RegisterValueChangedCallback(evt =>
            {
                _setter(AppController.Volumes.ToList().Find(v => v.Name == evt.newValue));
                onValueChanged?.Invoke();
            });
            
            return elem;
        }

        public void BindTo(VisualElement element)
        {
            if (element is not DropdownField dropdown) return;
            // ?
        }
    }
}