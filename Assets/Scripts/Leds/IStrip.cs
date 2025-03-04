using System.Collections.Generic;
using Leds.UI;
using UnityEngine;
using Volumes;

namespace Leds
{
    public interface IStrip : IUIGenerator
    {
        string Name { get; set; }
        List<Pixel> Pixels { get; }
        Transform Transform { get; set; }
        List<VolumeController> Volumes { get; set; }
        VolumeController CurrentVolume { get; set; }
        bool Visualize { get; set; }

        void SetPixelPrefab(GameObject prefab);
    }
}