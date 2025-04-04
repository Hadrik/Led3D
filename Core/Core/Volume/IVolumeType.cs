using System.Numerics;
using ColorHelper;
using Led3D_2.Utility;

namespace Led3D_2.Core.Volume;

/// <summary>
/// All VolumeTypes need to implement this interface.
/// </summary>
public interface IVolumeType : ISettingsProvider
{
    HSV GetColorAt(Vector3 position);
    VolumePositionData GetVolumePositionData();
    event Action RedrawVisualization;
}

public class VolumePositionData
{
    public string Id { get; set; }
    public required Vector3 Position { get; set; }
    public required Vector3 Rotation { get; set; }
    public required Vector3 Scale { get; set; }
}