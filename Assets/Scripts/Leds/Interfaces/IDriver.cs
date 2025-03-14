using System;
using System.Collections.Generic;
using UI;

namespace Leds.Interfaces
{
    public interface IDriver : IUIProvider
    {
        string Name { get; }
        IReadOnlyList<IStrip> Strips { get; }
        bool Visualize { get; set; }
        
        // Events
        event Action<IStrip> StripAdded;
        event Action<IStrip> StripRemoved;
        event Action VisualizationChanged;
        
        // Methods
        void AddStrip<T>() where T : IStrip;
        void RemoveStrip(IStrip strip);
    }
}