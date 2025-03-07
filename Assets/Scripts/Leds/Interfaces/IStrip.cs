using System;
using System.Collections.Generic;
using UI;
using UnityEngine;

namespace Leds
{
    public interface IStrip
    {
        string Name { get; set; }
        bool Visualize { get; set; }
        IReadOnlyList<Pixel> Pixels { get; }
        int PixelCount { get; set; }

        // Events
        event Action<int> PixelCountChanged;
        event Action VisualizationChanged;
        event Action NameChanged;
        
        // Methods
        void SetPixelPrefab(GameObject prefab);
        // void Destroy();
    }
}