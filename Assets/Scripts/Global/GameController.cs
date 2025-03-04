using System.Collections.Generic;
using Leds;
using Leds.UI;
using Volumes;
using UnityEngine;
using UnityEngine.UIElements;

namespace Global
{
    public class GameController : MonoBehaviour, IUIGenerator
    {
        public List<Driver> drivers = new();
        public List<VolumeController> volumeControllers = new();

        private void AddDriver()
        {
            var pf = Resources.Load<GameObject>("Prefabs/Driver");
            var obj = Instantiate(pf, new Vector3(0, 0, 0), Quaternion.identity, transform);
            obj.name = $"Driver_{drivers.Count}";
            var d = obj.AddComponent<Driver>();
            drivers.Add(d);
        }

        private void AddVolume()
        {
            var pf = Resources.Load<GameObject>("Prefabs/VolumeController");
            var obj = Instantiate(pf, new Vector3(0, 0, 0), Quaternion.identity, transform);
            obj.name = $"VolumeController_{volumeControllers.Count}";
            var d = obj.AddComponent<VolumeController>();
            volumeControllers.Add(d);
        }

        public VisualElement GenerateUI()
        {
            var template = Resources.Load<VisualTreeAsset>("UI/SelectionPanelTemplate");
            var driverTemplate = Resources.Load<VisualTreeAsset>("UI/DriverTemplate");
            // var volumeTemplate = Resources.Load<VisualTreeAsset>("UI/VolumeControllerTemplate");
            
            var root = template.Instantiate();
            
            var driverList = root.Q<ListView>("DriversList");
            var addDriverButton = root.Q<Button>("AddDriver");
            // var volumeList = root.Q<ListView>("VolumesList");
            // var addVolumeButton = root.Q<Button>("AddVolume");
            
            driverList.itemsSource = drivers;
            driverList.makeItem = () => driverTemplate.Instantiate();
            driverList.bindItem = (element, i) =>
            {
                element.Clear();
                element.Add(drivers[i].GenerateUI());
            };
            
            addDriverButton.clicked += () =>
            {
                AddDriver();
                driverList.Rebuild();
            };
            
            // volumeList.itemsSource = volumeControllers;
            // volumeList.makeItem = () => volumeTemplate.Instantiate();
            // volumeList.bindItem = (element, i) =>
            // {
            //     element.Clear();
            //     element.Add(volumeControllers[i].GenerateUI());
            // };
            
            // addVolumeButton.clicked += () =>
            // {
            //     AddVolume();
            //     volumeList.Rebuild();
            // };
            
            return root;
        }
    }
}