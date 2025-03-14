using System;
using System.Collections.Generic;
using Tools;
using UI.ControlElementDescriptors;
using UnityEngine;
using Volumes.Interfaces;

namespace Volumes
{
    public abstract class BaseVolume : MonoBehaviour, IVolume
    {
        public string Name => name;

        public event Action VisualizationChanged;

        [SerializeProperty, SerializeField] private bool visualize;

        public bool Visualize
        {
            get => visualize;
            set
            {
                visualize = value;
                VisualizationChanged?.Invoke();
            }
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }

        public virtual IEnumerable<IControlDescriptor> GetUIControls()
        {
            yield return new ToggleCD(
                "Visualize",
                () => Visualize,
                b => Visualize = b);
        }

        public abstract Color SampleColorAt(Vector3 worldPosition);

        public bool Contains(Vector3 localPosition)
        {
            return Mathf.Abs(localPosition.x) < transform.localScale.x &&
                   Mathf.Abs(localPosition.y) < transform.localScale.y &&
                   Mathf.Abs(localPosition.z) < transform.localScale.z;
        }

        public bool ContainsW(Vector3 worldPosition)
        {
            return Contains(WorldToLocal(worldPosition));
        }

        private void OnDrawGizmos()
        {
            if (!visualize) return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }

        protected Vector3 WorldToLocal(Vector3 worldPosition)
        {
            return transform.InverseTransformPoint(worldPosition);
        }

        protected Vector3 Normalized(Vector3 localPosition)
        {
            return new Vector3(localPosition.x / transform.localScale.x, localPosition.y / transform.localScale.y,
                localPosition.z / transform.localScale.z);
        }

        protected Vector3 NormalizedW(Vector3 worldPosition)
        {
            return Normalized(WorldToLocal(worldPosition));
        }
    }
}