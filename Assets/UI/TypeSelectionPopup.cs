using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class TypeSelectionPopup
    {
        private static readonly VisualTreeAsset Template = Resources.Load<VisualTreeAsset>("UI/TypeSelectionPopupTemplate");
        
        private readonly VisualElement _root;
        private Action<Type> _onTypeSelected;

        public TypeSelectionPopup(IEnumerable<Type> types, string title)
        {
            _root = Template.Instantiate();
            _root.style.position = Position.Absolute;
            _root.style.width = 300;
            _root.style.height = 400;
            
            _root.Q<Label>("Title").text = title;
            
            FillList(types.ToList());
            
            var cancelButton = _root.Q<Button>("Cancel");
            cancelButton.clicked += () =>
            {
                _root.RemoveFromHierarchy();
            };
        }
        
        public void Show(VisualElement parent, Action<Type> onTypeSelected)
        {
            _root.style.left = (parent.layout.width - 300) / 2;
            _root.style.top = (parent.layout.height - 400) / 2;
            
            _onTypeSelected = onTypeSelected;
            parent.Add(_root);
        }

        private void FillList(List<Type> types)
        {
            var list = _root.Q<ListView>("List");

            list.itemsSource = types.ToList();
            list.makeItem = () => new Label();
            list.bindItem = (element, i) =>
            {
                var type = types.ElementAt(i);
                var elem = (Label)element;
                elem.text = type.Name;
                elem.style.alignSelf = Align.Center;
                element.RegisterCallback<MouseDownEvent>(evt =>
                {
                    evt.StopPropagation();
                    _root.RemoveFromHierarchy();
                    _onTypeSelected?.Invoke(type);
                });
            };
        }
    }
}