using UnityEngine.UIElements;

namespace UI
{
    public static class UIBuilder
    {
        public static VisualElement Build(IUIProvider target)
        {
            var root = new VisualElement();

            foreach (var control in target.GetUIControls())
            {
                var elem = control.CreateElement(null);
                root.Add(elem);
            }
            
            return root;
        }
        
        public static void Update(VisualElement root, IUIProvider target)
        {
            foreach (var control in target.GetUIControls())
            {
                var elem = root.Q(control.Label);
                if (elem is null) continue;
                control.BindTo(elem);
            }
        }
    }
}