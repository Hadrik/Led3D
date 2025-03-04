using JetBrains.Annotations;
using UnityEngine;

namespace Leds
{
    public delegate void OnMove();
    public class Handle : MonoBehaviour
    {
        [CanBeNull] public event OnMove OnMove;
        
        public static Handle Create(Transform parent, OnMove onMove = null, string name = null)
        {
            var prefab = Resources.Load<GameObject>("Prefabs/Handle");
            var go = Instantiate(prefab, parent);
            if (name is not null) go.name = name;
            var comp = go.AddComponent<Handle>();
            comp.OnMove = onMove;
            return comp;
        }

        private void Update()
        {
            if (!transform.hasChanged) return;
            OnMove?.Invoke();
            transform.hasChanged = false;
        }
    }
}