using UnityEngine;

namespace Volumes
{
    public interface IVolume
    {
        Color SampleColorAt(Vector3 position);
        bool Contains(Vector3 position);
    }
}
