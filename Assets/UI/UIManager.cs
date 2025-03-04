using Global;
using Leds;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        private UIDocument _document;
        private GameController _gameController;
        private VisualElement _root;
        
        private void Awake()
        {
            _document = GetComponent<UIDocument>();
            if (_document == null)
            {
                _document = gameObject.AddComponent<UIDocument>();
            }
            _root = _document.rootVisualElement;

            _gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
        }

        private void Start()
        {
            var template = Resources.Load<VisualTreeAsset>("UI/Main");
            var main = template.Instantiate();
            
            var selection = main.Q<ScrollView>("Selection");
            var detail = main.Q<ScrollView>("Detail");
            
            selection.Add(_gameController.GenerateUI());
            detail.SetEnabled(false);
            detail.style.display = DisplayStyle.None;
            
            _root.Add(main);
        }

        public void ShowStripDetail(IStrip strip)
        {
            var detail = strip.GenerateUI();
            var detailView = _root.Q<ScrollView>("Detail");
            detailView.Clear();
            detailView.Add(detail);
            detailView.SetEnabled(true);
            detailView.style.display = DisplayStyle.Flex;
        }
    }
}