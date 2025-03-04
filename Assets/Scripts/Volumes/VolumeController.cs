using Tools;
using Unity.VisualScripting;
using UnityEngine;

namespace Volumes
{
    public class VolumeController : MonoBehaviour
    {
        [SerializeField] private bool visualize;
        
        private Vector3 _size;

        private IVolume _activeVolume;
        private IVolume ActiveVolume
        {
            get => _activeVolume;
            set => _activeVolume = value;
        }
        
        private void Start()
        {
            _size = new Vector3(2, 2, 2);
            ActiveVolume = new VolumeType.Gradient(transform.position, _size);
        }

        public Color SampleColorAt(Vector3 worldPosition)
        {
            if (ActiveVolume is null)
            {
                Debug.LogError("No active volume set");
                return Color.magenta;
            }
            return ActiveVolume.SampleColorAt(worldPosition);
        }

        private void OnDrawGizmos()
        {
            if (!visualize) return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, _size);
        }
    }
}
