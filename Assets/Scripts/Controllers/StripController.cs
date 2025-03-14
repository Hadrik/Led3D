using System.Linq;
using Leds.Interfaces;
using UI;
using UnityEngine.UIElements;

namespace Controllers
{
    public class StripController : BaseEntityController<IStrip>
    {
        public override string Name => Entity.Name ?? "Unknown Strip";
        public override SelectableType SelectionType => SelectableType.Strip;
        
        public StripController(SelectionManager selectionManager, IStrip strip) : base(selectionManager, strip)
        {
        }

        public override VisualElement CreateSelectionView()
        {
            var elem = new Label();
            elem.text = Name;
            Entity.NameChanged += OnNameChanged;
            
            elem.RegisterCallback<MouseDownEvent>(evt =>
            {
                SelectionManager.Select(this);
                evt.StopPropagation();
            });
            
            SelectionElement = elem;
            return elem;
        }

        public override VisualElement CreateDetailView()
        {
            var elem = new VisualElement();
            
            var controls = UIBuilder.Build(Entity);
            elem.Add(controls);
            
            elem.Add(CreatePixelList(Entity));
            
            DetailElement = elem;
            return elem;
        }
        
        public override void Refresh()
        {
            if (DetailElement == null) return;
            DetailElement.Clear();
            UIBuilder.Update(DetailElement, Entity);
        }
        
        public override void Destroy()
        {
            Entity.NameChanged -= OnNameChanged;
        }
        
        private VisualElement CreatePixelList(IStrip strip)
        {
            // TODO: List needs to get rebuilt on pixel count change
            var list = new ListView();
            list.itemsSource = strip.Pixels.ToList();
            list.makeItem = () => new VisualElement();
            list.bindItem = (element, i) =>
            {
                element.Clear();
                element.Add(UIBuilder.Build(strip.Pixels[i]));
            };
            return list;
        }
        
        // Event handlers
        private void OnNameChanged()
        {
            SelectionElement.Q<Label>().text = Name;
        }
    }
}