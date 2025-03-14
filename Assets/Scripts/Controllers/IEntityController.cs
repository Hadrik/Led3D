using UI;
using UnityEngine.UIElements;

namespace Controllers
{
    public interface IEntityController
    {
        string Name { get; }
        SelectableType SelectionType { get; }
        VisualElement CreateSelectionView();
        VisualElement CreateDetailView();
        void Refresh();
        void Destroy();
    }
}