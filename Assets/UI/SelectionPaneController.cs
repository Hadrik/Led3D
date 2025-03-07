using System.Collections.Generic;
using Global;
using Leds.Interfaces;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class SelectionPaneController
    {
        private static readonly VisualTreeAsset SelectionPaneAsset = Resources.Load<VisualTreeAsset>("UI/SelectionPaneTemplate");
        private static readonly AppController AppController = GameObject.FindWithTag("GameController").GetComponent<AppController>();
        
        private readonly Dictionary<IDriver, DriverUIController> _driverControllers = new();
        // private readonly Dictionary<VolumeController, VolumeUIController> _volumeControllers = new();
        
        private ListView _driversList;
        private ListView _volumesList;
        
        private readonly SelectionManager _selectionManager;
        
        public SelectionPaneController(SelectionManager selectionManager)
        {
            _selectionManager = selectionManager;
            AppController.DriverAdded += OnDriverAdded;
            AppController.DriverRemoved += OnDriverRemoved;
        }
        
        public VisualElement Create()
        {
            var instance = SelectionPaneAsset.Instantiate();
            
            _driversList = instance.Q<ListView>("DriversList");
            // var volumes = instance.Q<ListView>("VolumesList"); TODO: Implement Volumes
            var driverAdd = instance.Q<Button>("AddDriver");
            // var volumeAdd = instance.Q<Button>("AddVolume");
            
            driverAdd.clicked += AppController.AddDriver;
            
            CreateDriverList(_driversList, AppController);
            
            
            return instance;
        }

        public void Destroy()
        {
            AppController.DriverAdded -= OnDriverAdded;
            AppController.DriverRemoved -= OnDriverRemoved;
            
            foreach (var controller in _driverControllers.Values)
            {
                controller.Destroy();
            }
            _driverControllers.Clear();
        }

        public void Refresh()
        {
            _driversList.Rebuild();
        }
        
        private void CreateDriverList(ListView list, AppController app)
        {
            list.itemsSource = app.drivers;
            list.makeItem = () => new VisualElement();
            list.bindItem = (element, i) =>
            {
                element.Clear();
                var driver = app.drivers[i];
        
                if (!_driverControllers.TryGetValue(driver, out var controller))
                {
                    controller = new DriverUIController(_selectionManager, driver);
                    _driverControllers.Add(driver, controller);
                }
                
                var view = controller.CreateSelectionView();
                view.Q<Button>("Remove").clicked += () => RemoveDriver(driver);
                
                element.Add(view);
            };
        }

        private void RemoveDriver(IDriver driver)
        {
            AppController.RemoveDriver(driver);
        }
        
        // Event handlers
        private void OnDriverAdded(IDriver driver)
        {
            Refresh();
        }
        
        private void OnDriverRemoved(IDriver driver)
        {
            if (_driverControllers.TryGetValue(driver, out var controller))
            {
                controller.Destroy();
                _driverControllers.Remove(driver);
            }
            
            Refresh();
        }
        
        // private void OnVolumeAdded(VolumeController volume)
        // {
        // }
        
        // private void OnVolumeRemoved(VolumeController volume)
        // {
        // }
    }
}