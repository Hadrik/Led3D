using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        private static VisualTreeAsset _rootAsset;
        
        
        private UIDocument _document;
        private DetailPaneController _detailPaneController;
        private SelectionPaneController _selectionPaneController;
        private SelectionManager _selectionManager;
        private ScrollView _detailView;
        
        private void Awake()
        {
            _rootAsset = Resources.Load<VisualTreeAsset>("UI/RootTemplate");
        }
        
        private void Start()
        {
            _document = GetComponent<UIDocument>();
            _selectionManager = new SelectionManager();
            _selectionManager.SelectionChanged += OnSelectionChanged;
            
            _detailPaneController = new DetailPaneController(_selectionManager);
            _selectionPaneController = new SelectionPaneController(_selectionManager);
            
            var root = _rootAsset.Instantiate();
            var selection = root.Q<ScrollView>("Selection");
            _detailView = root.Q<ScrollView>("Detail");
            DetailHide();
            
            selection.Add(_selectionPaneController.Create());
            _detailView.Add(_detailPaneController.Create());
            
            root.style.height = Length.Percent(100);
            _document.rootVisualElement.Add(root);
        }
        
        private void OnDestroy()
        {
            _detailPaneController.Destroy();
            _selectionPaneController.Destroy();
        }

        private void OnSelectionChanged(ISelectable selectable)
        {
            if (selectable is null)
            {
                DetailHide();
            }
            else
            {
                DetailShow();
            }
        }

        private void DetailShow()
        {
            _detailView.style.display = DisplayStyle.Flex;
        }

        private void DetailHide()
        {
            _detailView.style.display = DisplayStyle.None;
        }
    }
}