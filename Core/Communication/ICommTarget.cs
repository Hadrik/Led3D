using System.Net;
using ColorHelper;
using Led3D_2.Core.Driver;
using Led3D_2.Utility;

namespace Led3D_2.Communication;

public interface ICommTarget : ISettingsProvider
{
    bool Connect(IPAddress address, int? port = null);
    bool Send(DriverColorData colorData);
    void Disconnect();
}