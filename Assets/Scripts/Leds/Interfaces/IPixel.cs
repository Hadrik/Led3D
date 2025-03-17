using System;
using UI;
using UnityEngine;
using Volumes.Interfaces;

namespace Leds.Interfaces
{
    public interface IPixel : IUIProvider
    {
        int Index { get; }
        Color Color { get; }
        Vector3 Position { get; }
        bool Visualize { get; }
        IVolume Volume { get; }
        
        // Events
        event Action<Color> ColorChanged;
    }
}