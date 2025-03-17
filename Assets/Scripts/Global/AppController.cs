using System;
using System.Collections.Generic;
using Leds;
using Leds.Interfaces;
using Volumes;
using Volumes.Interfaces;
using UnityEngine;

namespace Global
{
    public class AppController : MonoBehaviour
    {
        public IReadOnlyList<Type> AvailableDriverTypes => _availableDriverTypes;
        private readonly List<Type> _availableDriverTypes = new();
        
        public IReadOnlyList<Type> AvailableVolumeTypes => _availableVolumeTypes;
        private readonly List<Type> _availableVolumeTypes = new();
        
        public IReadOnlyList<IDriver> Drivers => _drivers;
        private readonly List<Driver> _drivers = new();
        public IReadOnlyList<IVolume> Volumes => _volumes;
        private readonly List<BaseVolume> _volumes = new();
        
        public event Action<IDriver> DriverAdded;
        public event Action<IDriver> DriverRemoved;
        public event Action VolumeAdded;
        public event Action VolumeRemoved;

        private void Awake()
        {
            _availableDriverTypes.AddRange(TypeLoader.GetDerivedType<IDriver>());
            _availableVolumeTypes.AddRange(TypeLoader.GetDerivedType<IVolume>());
        }

        public void AddDriver(Type driverType)
        {
            var pf = Resources.Load<GameObject>("Prefabs/Driver");
            var obj = Instantiate(pf, new Vector3(0, 0, 0), Quaternion.identity, transform);
            obj.name = $"Driver_{_drivers.Count}";
            var d = (Driver)obj.AddComponent(driverType);
            _drivers.Add(d);
            DriverAdded?.Invoke(d);
        }

        public void RemoveDriver(IDriver driver)
        {
            var found = _drivers.Find(d => ReferenceEquals(d, driver));
            if (found is null)
            {
                Debug.LogError("App Controller tried to remove non existing driver");
                return;
            }
            found.Destroy();
            _drivers.Remove(found);
            DriverRemoved?.Invoke(driver);
        }

        public void AddVolume(Type volumeType)
        {
            var pf = Resources.Load<GameObject>("Prefabs/VolumeController");
            var obj = Instantiate(pf, new Vector3(0, 0, 0), Quaternion.identity, transform);
            obj.name = $"Volume_{_volumes.Count}";
            var d = (BaseVolume)obj.AddComponent(volumeType);
            _volumes.Add(d);
            VolumeAdded?.Invoke();
        }
        
        public void RemoveVolume(IVolume baseVolume)
        {
            var found = _volumes.Find(d => ReferenceEquals(d, baseVolume));
            if (found is null)
            {
                Debug.LogError("App Controller tried to remove non existing volume controller");
                return;
            }
            found.Destroy();
            _volumes.Remove(found);
            VolumeRemoved?.Invoke();
        }
    }
}