using System;
using UnityEngine;

namespace Leds.Interfaces
{
    public interface IPixel
    {
        int Index { get; }
        Color Color { get; }
        Vector3 Position { get; }
        bool Visualize { get; }
        
        // Events
        event Action<Color> ColorChanged;
    }
}