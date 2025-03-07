using System;
using System.Collections.Generic;
using Leds;
using Leds.Interfaces;
using Volumes;
using UnityEngine;

namespace Global
{
    public class AppController : MonoBehaviour
    {
        public List<Driver> drivers = new();
        public List<VolumeController> volumeControllers = new();
        
        public event Action<IDriver> DriverAdded;
        public event Action<IDriver> DriverRemoved;
        public event Action VolumeAdded;
        public event Action VolumeRemoved;

        public void AddDriver()
        {
            var pf = Resources.Load<GameObject>("Prefabs/Driver");
            var obj = Instantiate(pf, new Vector3(0, 0, 0), Quaternion.identity, transform);
            obj.name = $"Driver_{drivers.Count}";
            var d = obj.AddComponent<Driver>();
            drivers.Add(d);
            DriverAdded?.Invoke(d);
        }

        public void RemoveDriver(IDriver driver)
        {
            var found = drivers.Find(d => ReferenceEquals(d, driver));
            if (found is null)
            {
                Debug.LogError("App Controller tried to remove non existing driver");
                return;
            }
            found.Destroy();
            drivers.Remove(found);
            DriverRemoved?.Invoke(driver);
        }

        public void AddVolume()
        {
            var pf = Resources.Load<GameObject>("Prefabs/VolumeController");
            var obj = Instantiate(pf, new Vector3(0, 0, 0), Quaternion.identity, transform);
            obj.name = $"VolumeController_{volumeControllers.Count}";
            var d = obj.AddComponent<VolumeController>();
            volumeControllers.Add(d);
            VolumeAdded?.Invoke();
        }
    }
}