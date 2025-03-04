using System.Collections.Generic;
using System.Linq;
using Leds.UI;
using Tools;
using UI;
using UnityEngine;
using UnityEngine.UIElements;
using Linear = Leds.StripType.Linear;

namespace Leds
{
    public class Driver : MonoBehaviour, IUIGenerator
    {
        private List<IStrip> _strips = new();
        private VisualElement _myUI;
        private UIManager _manager;

        [SerializeField, SerializeProperty] private bool visualize;

        private bool Visualize
        {
            get => visualize;
            set
            {
                visualize = value;
                foreach (var strip in _strips)
                {
                    strip.Visualize = visualize;
                }
            }
        }


        [SerializeField, SerializeProperty] private GameObject visualisationPrefab;
        private GameObject VisualisationPrefab
        {
            get => visualisationPrefab;
            set
            {
                visualisationPrefab = value;
                foreach (var strip in _strips)
                {
                    strip.SetPixelPrefab(visualisationPrefab);
                }
            }
        }

        private void Start()
        {
            _manager = GameObject.FindWithTag("UI Manager").GetComponent<UIManager>();
            var pf = Resources.Load<GameObject>("Prefabs/PixelVisualization");
            if (pf is null)
            {
                Debug.LogError("Prefabs/PixelVisualization could not be loaded");
            }
            else
            {
                VisualisationPrefab = pf;
            }
        }

        private void AddStrip<T>() where T : IStrip
        {
            var pf = Resources.Load<GameObject>("Prefabs/Strip");
            var strip = Instantiate(pf, transform);
            strip.name = $"Strip_{_strips.Count}";

            var component = strip.AddComponent<Linear>();
            if (VisualisationPrefab is not null)
            {
                component.SetPixelPrefab(visualisationPrefab);
            }

            _strips.Add(component);
            
            _myUI?.Q<ListView>("StripsList").Rebuild();
        }
        
        public VisualElement GenerateUI()
        {
            var template = Resources.Load<VisualTreeAsset>("UI/DriverTemplate");
            var stripTemplate = Resources.Load<VisualTreeAsset>("UI/StripNameTemplate");
            var root = template.Instantiate();
            
            var stripList = root.Q<ListView>("StripsList");
            var addStripButton = root.Q<Button>("AddStrip");
            
            stripList.itemsSource = _strips;
            stripList.makeItem = () => stripTemplate.Instantiate();
            stripList.bindItem = (element, i) =>
            {
                var n = element.Q<Label>("Name");
                n.text = _strips[i].Name;
                // element.Add(_strips[i].GenerateUI());
            };
            stripList.selectionChanged += selection =>
            {
                var stripName = ((IStrip)selection.First()).Name;
                var strip = _strips.Find(strip => strip.Name == stripName);
                _manager.ShowStripDetail(strip);
            };
            
            addStripButton.clicked += () =>
            {
                AddStrip<Linear>();
                stripList.Rebuild();
            };
            
            _myUI = root;
            return root;
        }
    }
}