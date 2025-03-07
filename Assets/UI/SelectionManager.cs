using System;
using UnityEngine;

namespace UI
{
    public class SelectionManager
    {
        private ISelectable _selected;

        public event Action<ISelectable> SelectionChanged;

        public void Select(ISelectable selectable)
        {
            if (_selected == selectable)
            {
                if (_selected is null)
                {
                    Debug.LogError("Trying to select null selectable");
                    return;
                }
                ClearSelection();
                return;
            }
            
            _selected = selectable;
            SelectionChanged?.Invoke(_selected);
        }

        public void ClearSelection()
        {
            Select(null);
        }
        
        public ISelectable GetSelected()
        {
            return _selected;
        }
    }
}