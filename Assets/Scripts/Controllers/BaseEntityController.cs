using UI;
using UnityEngine.UIElements;

namespace Controllers
{
    public abstract class BaseEntityController<T> : IEntityController, ISelectable
    {
        protected readonly SelectionManager SelectionManager;
        protected readonly T Entity;
        protected VisualElement SelectionElement;
        protected VisualElement DetailElement;
        
        public abstract string Name { get; }
        public abstract SelectableType SelectionType { get; }
        
        protected BaseEntityController(SelectionManager selectionManager, T entity)
        {
            this.SelectionManager = selectionManager;
            this.Entity = entity;
        }
        
        public abstract VisualElement CreateSelectionView();
        public abstract VisualElement CreateDetailView();
        public abstract void Refresh();
        public abstract void Destroy();
    }
}