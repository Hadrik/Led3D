using System.Collections.Generic;
using System.Linq;
using Leds.Interfaces;
using UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Controllers
{
    public class DriverController : BaseEntityController<IDriver>
    {
        private static readonly StyleSheet Style = Resources.Load<StyleSheet>("UI/DriverController");
        
        public override string Name => Entity.Name;
        public override SelectableType SelectionType => SelectableType.Driver;

        private readonly Dictionary<IStrip, StripController> _stripControllers = new();
        private ListView _listView;
        
        public DriverController(SelectionManager selectionManager, IDriver driver) : base(selectionManager, driver)
        {
        }
        
        public override VisualElement CreateSelectionView()
        {
            Entity.StripAdded += OnStripAdded;
            Entity.StripRemoved += OnStripRemoved;
            
            var elem = new VisualElement();
            elem.AddToClassList("driver-selection");
            elem.styleSheets.Add(Style);
            
            var topRow = new VisualElement();
            var label = new Label();
            label.text = Name;
            topRow.Add(label);
            
            var button = new Button();
            button.text = "X";
            button.name = "Remove";
            topRow.Add(button);
            topRow.AddToClassList("top-row");
            elem.Add(topRow);
            
            elem.RegisterCallback<MouseDownEvent>(evt =>
            {
                SelectionManager.Select(this);
                evt.StopPropagation();
            });
            
            _listView = CreateStripList(Entity);
            elem.Add(_listView);
            
            var addStripButton = new Button(() =>
            {
                Entity.AddStrip<Leds.StripType.Linear>();
            });
            addStripButton.text = "Add Strip";
            elem.Add(addStripButton);
            
            SelectionElement = elem;
            return elem;
        }
        
        public override VisualElement CreateDetailView()
        {
            var elem = new VisualElement();
            
            var controls = UIBuilder.Build(Entity);
            elem.Add(controls);
            
            DetailElement = elem;
            return elem;
        }
        
        public override void Refresh()
        {
            if (_listView is null) return;
            
            _listView.itemsSource = Entity.Strips.ToList();
            _listView.Rebuild();
        }
        
        public override void Destroy()
        {
            foreach (var controller in _stripControllers.Values)
            {
                controller.Destroy();
            }
            _stripControllers.Clear();
            
            Entity.StripAdded -= OnStripAdded;
            Entity.StripRemoved -= OnStripRemoved;
        }
        
        // Event handlers
        private void OnStripAdded(IStrip strip)
        {
            Refresh();
        }
        
        private void OnStripRemoved(IStrip obj)
        {
            if (_stripControllers.TryGetValue(obj, out var controller))
            {
                controller.Destroy();
                _stripControllers.Remove(obj);
            }
            
            Refresh();
        }
        
        private ListView CreateStripList(IDriver driver)
        {
            var list = new ListView();
            list.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            list.focusable = false;
            
            list.itemsSource = driver.Strips.ToList();
            list.makeItem = () => new VisualElement();
            list.bindItem = (element, i) =>
            {
                element.Clear();
                var strip = driver.Strips[i];
                
                if (!_stripControllers.TryGetValue(strip, out var controller))
                {
                    controller = new StripController(SelectionManager, strip);
                    _stripControllers.Add(strip, controller);
                }
                
                var view = controller.CreateSelectionView();
                element.Add(view);
            };
            return list;
        }
    }
}