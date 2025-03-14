using System;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using Global;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class UIController : MonoBehaviour
    {
        private readonly Dictionary<object, IEntityController> _controllers = new();
        private AppController _appController;
        private ControllerFactory _factory;
        private SelectionManager _selectionManager;

        private static StyleSheet _topRowStyle;
        private static VisualTreeAsset _rootAsset;
        private UIDocument _document;
        private VisualElement _selectionPane;
        private VisualElement _detailPane;

        private void Awake()
        {
            _topRowStyle = Resources.Load<StyleSheet>("UI/DetailPaneTopRow");
            _rootAsset = Resources.Load<VisualTreeAsset>("UI/RootTemplate");
            _selectionManager = new SelectionManager();
            _factory = new ControllerFactory(_selectionManager);
            _selectionManager.SelectionChanged += OnSelectionChanged;
            _appController = GameObject.FindWithTag("GameController").GetComponent<AppController>();
        }

        private void Start()
        {
            _document = GetComponent<UIDocument>();
            var root = _rootAsset.Instantiate();

            _selectionPane = root.Q("Selection");
            _detailPane = root.Q("Detail");

            SetupDriversSection();
            SetupVolumesSection();

            _detailPane.style.display = DisplayStyle.None;
            root.style.height = Length.Percent(100);
            _document.rootVisualElement.Add(root);
        }

        private void OnDestroy()
        {
            _selectionManager.SelectionChanged -= OnSelectionChanged;

            foreach (var controller in _controllers.Values)
            {
                controller.Destroy();
            }

            _controllers.Clear();
        }

        private void OnSelectionChanged(ISelectable selectable)
        {
            if (selectable is null)
            {
                _detailPane.style.display = DisplayStyle.None;
                return;
            }

            _detailPane.Clear();

            if (selectable is IEntityController controller)
            {
                var toprow = new VisualElement();
                toprow.Add(new Label(controller.Name));
                toprow.Add(new Button(() => { _selectionManager.ClearSelection(); }) { text = "X" });
                toprow.styleSheets.Add(_topRowStyle);
                _detailPane.Add(toprow);
                _detailPane.Add(controller.CreateDetailView());
                _detailPane.style.display = DisplayStyle.Flex;
            }
        }

        private IEntityController GetOrCreateEntity<T>(T entity)
        {
            if (_controllers.TryGetValue(entity, out var controller))
            {
                return controller;
            }

            controller = _factory.Create(entity);
            _controllers.Add(entity, controller);
            return controller;
        }

        private void SetupDriversSection()
        {
            var driversPane = _selectionPane.Q<Foldout>("Drivers");
            var driversList = driversPane.Q<ListView>("DriversList");
            var driversButton = driversPane.Q<Button>("AddDriver");

            driversList.itemsSource = _appController.Drivers.ToList();
            driversList.makeItem = () => new VisualElement();
            driversList.bindItem = (element, i) =>
            {
                element.Clear();
                var driver = _appController.Drivers.ToList()[i];
                var controller = GetOrCreateEntity(driver);
                element.Add(controller.CreateSelectionView());
            };

            driversButton.clicked += () =>
            {
                var drivers = _appController.AvailableDriverTypes;
                if (drivers.Count == 1)
                {
                    AddDriver(driversList, drivers[0]);
                    return;
                }

                new TypeSelectionPopup(drivers, "Select driver type")
                    .Show(_document.rootVisualElement, (type) => { AddDriver(driversList, type); });
            };
        }

        private void AddDriver(ListView list, Type driverType)
        {
            _appController.AddDriver(driverType);
            list.itemsSource = _appController.Drivers.ToList();
            list.Rebuild();
        }

        private void SetupVolumesSection()
        {
            var volumesPane = _selectionPane.Q<Foldout>("Volumes");
            var volumesList = volumesPane.Q<ListView>("VolumesList");
            var volumesButton = volumesPane.Q<Button>("AddVolume");

            volumesList.itemsSource = _appController.Volumes.ToList();
            volumesList.makeItem = () => new VisualElement();
            volumesList.bindItem = (element, i) =>
            {
                element.Clear();
                var volume = _appController.Volumes.ToList()[i];
                var controller = GetOrCreateEntity(volume);
                element.Add(controller.CreateSelectionView());
            };

            volumesButton.clicked += () =>
            {
                var volumes = _appController.AvailableVolumeTypes;
                if (volumes.Count == 1)
                {
                    AddVolume(volumesList, volumes[0]);
                    return;
                }

                new TypeSelectionPopup(volumes, "Select volume type")
                    .Show(_document.rootVisualElement, (type) => { AddVolume(volumesList, type); });
            };
        }

        private void AddVolume(ListView list, Type type)
        {
            _appController.AddVolume(type);
            list.itemsSource = _appController.Volumes.ToList();
            list.Rebuild();
        }
    }
}