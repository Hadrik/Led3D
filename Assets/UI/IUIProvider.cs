using System.Collections.Generic;
using UI.ControlElementDescriptors;

namespace UI
{
    public interface IUIProvider
    {
        IEnumerable<IControlDescriptor> GetUIControls();
    }
}