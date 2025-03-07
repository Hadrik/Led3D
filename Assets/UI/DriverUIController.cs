using System.Collections.Generic;
using System.Linq;
using Leds;
using Leds.Interfaces;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class DriverUIController : ISelectable
    {
        public string Name => _driver.Name ?? "Unknown Driver";
        public SelectableType SelectionType => SelectableType.Driver;
        
        private static readonly VisualTreeAsset DriverSelectionAsset = Resources.Load<VisualTreeAsset>("UI/DriverSelectionTemplate");
        private static readonly VisualTreeAsset DriverDetailAsset = Resources.Load<VisualTreeAsset>("UI/DriverDetailTemplate");
        
        private readonly Dictionary<IStrip, StripUIController> _stripControllers = new();
        private readonly SelectionManager _selectionManager;
        private readonly IDriver _driver;
        
        private VisualElement _element;
        private ListView _listView;

        public DriverUIController(SelectionManager selectionManager, IDriver driver)
        {
            _driver = driver;
            _selectionManager = selectionManager;
        }
        
        public VisualElement CreateSelectionView()
        {
            var instance = DriverSelectionAsset.Instantiate();
            
            var name = instance.Q<Label>("Name");
            var addStrip = instance.Q<Button>("AddStrip");
            _listView = instance.Q<ListView>("StripsList");
            
            name.text = _driver.Name;
            
            // TODO: popup for strip type selection
            addStrip.clicked += _driver.AddStrip<Leds.StripType.Linear>;

            _driver.StripAdded += OnStripAdded;
            _driver.StripRemoved += OnStripRemoved;
            _driver.VisualizationChanged += OnVisualizationChanged;
            
            instance.RegisterCallback<MouseDownEvent>(evt =>
            {
                _selectionManager.Select(this);
                evt.StopPropagation();
            });
            
            CreateStripList(_listView, _driver);

            _element = instance;
            return instance;
        }

        public VisualElement CreateDetailView()
        {
            var instance = DriverDetailAsset.Instantiate();
            
            var visualize = instance.Q<Toggle>("Visualize");
            
            visualize.value = _driver.Visualize;
            visualize.RegisterValueChangedCallback(evt => _driver.Visualize = evt.newValue);
            
            return instance;
        }

        public void Refresh()
        {
            if (_driver == null || _listView == null) return;
            
            _listView.itemsSource = _driver.Strips.ToList();
            _listView.Rebuild();
        }
        
        public void Destroy()
        {
            if (_driver == null) return;
            _driver.StripAdded -= OnStripAdded;
            _driver.StripRemoved -= OnStripRemoved;
            _driver.VisualizationChanged -= OnVisualizationChanged;

            foreach (var controller in _stripControllers.Values)
            {
                controller.Destroy();
            }
            _stripControllers.Clear();
        }
        
        private void CreateStripList(ListView list, IDriver driver)
        {
            list.itemsSource = driver.Strips.ToList();
            list.makeItem = () => new VisualElement();
            list.bindItem = (element, i) =>
            {
                element.Clear();
                var strip = driver.Strips[i];

                if (!_stripControllers.TryGetValue(strip, out var controller))
                {
                    controller = new StripUIController(_selectionManager, strip);
                    _stripControllers.Add(strip, controller);
                }

                var view = controller.CreateSelectionView();
                view.Q<Button>("Remove").clicked += () => RemoveStrip(strip);
                
                element.Add(view);
            };
        }

        private void RemoveStrip(IStrip strip)
        {
            _driver.RemoveStrip(strip);
            _stripControllers.Remove(strip);
        }
        
        // Event handlers
        private void OnStripRemoved(IStrip obj)
        {
            if (_stripControllers.TryGetValue(obj, out var controller))
            {
                controller.Destroy();
                _stripControllers.Remove(obj);
            }
            
            Refresh();
        }

        private void OnStripAdded(IStrip obj)
        {
            Refresh();
        }
        
        private void OnVisualizationChanged()
        {
            var visualize = _element?.Q<Toggle>("Visualize");
            visualize?.SetValueWithoutNotify(_driver.Visualize);
        }
    }
}