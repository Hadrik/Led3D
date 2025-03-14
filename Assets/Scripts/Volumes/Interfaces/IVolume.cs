using System;
using UI;
using UnityEngine;

namespace Volumes.Interfaces
{
    public interface IVolume : IUIProvider
    {
        string Name { get; }
        bool Visualize { get; set; }
        
        // Methods
        Color SampleColorAt(Vector3 position);
        bool Contains(Vector3 position);
        
        // Events
        event Action VisualizationChanged;
    }
}
