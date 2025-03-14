using System;
using System.Collections.Generic;
using UI;
using UnityEngine;

namespace Leds.Interfaces
{
    public interface IStrip : IUIProvider
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
    }
}