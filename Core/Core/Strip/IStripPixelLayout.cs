using System.Numerics;

namespace Led3D_2.Core.Strip;

public interface IStripPixelLayout
{
    event Action<List<Vector3>> PixelPositionsChanged;
}