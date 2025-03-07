using System.Linq;
using Leds;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class StripUIController : ISelectable
    {
        public string Name => _strip.Name ?? "Unknown Strip";
        public SelectableType SelectionType => SelectableType.Strip;
        
        private static readonly VisualTreeAsset StripSelectionAsset = Resources.Load<VisualTreeAsset>("UI/StripSelectionTemplate");
        private static readonly VisualTreeAsset StripDetailAsset = Resources.Load<VisualTreeAsset>("UI/StripDetailTemplate");

        private readonly SelectionManager _selectionManager;
        private readonly IStrip _strip;
        
        private VisualElement _listElement;
        private VisualElement _detailElement;
        private ListView _detailListView;
        

        public StripUIController(SelectionManager selectionManager, IStrip strip)
        {
            _strip = strip;
            _selectionManager = selectionManager;
        }
        
        public VisualElement CreateSelectionView()
        {
            var instance = StripSelectionAsset.Instantiate();
            
            var name = instance.Q<Label>("Name");
            
            name.text = _strip.Name;
            _strip.NameChanged += OnNameChanged;

            instance.RegisterCallback<MouseDownEvent>(evt =>
            {
                _selectionManager.Select(this);
                evt.StopPropagation();
            });

            _listElement = instance;
            return instance;
        }

        public VisualElement CreateDetailView()
        {
            if (_strip == null) return null;
            
            var instance = StripDetailAsset.Instantiate();

            var length = instance.Q<SliderInt>("Count");
            var visualize = instance.Q<Toggle>("Visualize");
            _detailListView = instance.Q<ListView>("List");

            length.value = _strip.PixelCount;
            length.RegisterValueChangedCallback(evt => _strip.PixelCount = evt.newValue);
            
            visualize.value = _strip.Visualize;
            visualize.RegisterValueChangedCallback(evt => _strip.Visualize = evt.newValue);
            
            _strip.PixelCountChanged += OnPixelCountChanged;
            _strip.VisualizationChanged += OnVisualizationChanged;
            
            CreatePixelList(_detailListView, _strip);
            
            _detailElement = instance;
            return instance;
        }

        public void Refresh()
        {
            if (_strip == null || _detailListView == null) return;
            
            _detailListView.itemsSource = _strip.Pixels.ToList();
            _detailListView.Rebuild();
        }

        public void Destroy()
        {
            if (_strip == null) return;
            _strip.NameChanged -= OnNameChanged;
            _strip.PixelCountChanged -= OnPixelCountChanged;
            _strip.VisualizationChanged -= OnVisualizationChanged;
        }

        // Event handlers
        private void OnNameChanged()
        {
            var newName = _strip.Name;
            
            if (_listElement != null)
            {
                _listElement.Q<Label>("Name").text = newName;
            }
        }

        private void OnPixelCountChanged(int count)
        {
            Refresh();
        }
        
        private void OnVisualizationChanged()
        {
            var visualize = _detailElement?.Q<Toggle>("Visualize");
            visualize?.SetValueWithoutNotify(_strip.Visualize);
        }

        private static void CreatePixelList(ListView list, IStrip strip)
        {
            list.itemsSource = strip.Pixels.ToList();
            list.makeItem = () => new VisualElement();
            list.bindItem = (element, i) =>
            {
                element.Clear();
                element.Add(PixelUIController.Create(strip.Pixels[i]));
            };
        }
    }
}