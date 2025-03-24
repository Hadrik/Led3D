using System.Numerics;
using Led3D_2.Utility;

namespace Led3D_2.Core.Strip;

public interface IStripPixelLayout : ISettingsProvider
{
    event Action<List<Vector3>> PixelPositionsChanged;
}