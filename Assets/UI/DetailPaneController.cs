using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class DetailPaneController
    {
        private static readonly VisualTreeAsset DetailPaneAsset = Resources.Load<VisualTreeAsset>("UI/DetailPaneTemplate");

        private readonly SelectionManager _selectionManager;
        private VisualElement _instance;
        private VisualElement _container;

        public DetailPaneController(SelectionManager selectionManager)
        {
            _selectionManager = selectionManager;
            _selectionManager.SelectionChanged += OnSelectionChanged;
        }
        
        public VisualElement Create()
        {
            var instance = DetailPaneAsset.Instantiate();
            _container = instance.Q<VisualElement>("Container");
            
            var closeButton = instance.Q<Button>("Close");
            closeButton.clicked += () => _selectionManager.ClearSelection();
            
            _instance = instance;
            return instance;
        }
        
        public void Destroy()
        {
            _selectionManager.SelectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged(ISelectable selectable)
        {
            _container.Clear();
            if (selectable != null)
            {
                ShowDetail(selectable);
            }
        }

        private void ShowDetail(ISelectable selectable)
        {
            var detail = selectable.CreateDetailView();
            
            var name = _instance.Q<Label>("Name");
            name.text = selectable.Name;
            
            _container.Add(detail);
        }
    }
}