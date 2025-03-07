using UnityEngine.UIElements;

namespace UI
{
    public interface ISelectable
    {
        string Name { get; }
        VisualElement CreateDetailView();
        SelectableType SelectionType { get; }
    }
}