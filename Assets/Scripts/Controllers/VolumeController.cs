using System.Linq;
using Leds.Interfaces;
using UI;
using UI.ControlElementDescriptors;
using UnityEngine.UIElements;
using Volumes.Interfaces;

namespace Controllers
{
    public class VolumeController : BaseEntityController<IVolume>
    {
        public override string Name => Entity.Name ?? "Unknown Strip";
        public override SelectableType SelectionType => SelectableType.Volume;
        
        public VolumeController(SelectionManager selectionManager, IVolume volume) : base(selectionManager, volume)
        {
        }

        public override VisualElement CreateSelectionView()
        {
            var elem = new Label();
            elem.text = Name;
            // Entity.NameChanged += OnNameChanged;
            
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
            
            DetailElement = elem;
            return elem;
        }
        
        public override void Refresh()
        {
            if (DetailElement == null) return;
            DetailElement.Clear();
            UIBuilder.Update(DetailElement, Entity);
        }

        public override void Destroy() {}
    }
}