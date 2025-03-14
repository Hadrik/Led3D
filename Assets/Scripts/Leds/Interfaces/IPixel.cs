using System;
using UI;
using UnityEngine;
using Volumes;

namespace Leds.Interfaces
{
    public interface IPixel : IUIProvider
    {
        int Index { get; }
        Color Color { get; }
        Vector3 Position { get; }
        bool Visualize { get; }
        BaseVolume BaseVolume { get; }
        
        // Events
        event Action<Color> ColorChanged;
    }
}