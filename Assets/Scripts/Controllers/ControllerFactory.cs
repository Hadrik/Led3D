using System;
using Leds.Interfaces;
using UI;
using Volumes.Interfaces;

namespace Controllers
{
    public class ControllerFactory
    {
        private readonly SelectionManager _selectionManager;
        
        public ControllerFactory(SelectionManager selectionManager)
        {
            _selectionManager = selectionManager;
        }

        public IEntityController Create<T>(T entity)
        {
            return entity switch
            {
                IStrip strip => new StripController(_selectionManager, strip),
                IDriver driver => new DriverController(_selectionManager, driver),
                IVolume volume => new VolumeController(_selectionManager, volume),
                _ => throw new ArgumentException($"No controller for entity of type {typeof(T).Name}")
            };
        }
    }
}