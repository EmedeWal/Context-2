namespace Context.UI
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class InputSprite : MonoBehaviour
    {
        [SerializeField] private InputSpriteContainer _container;
        [SerializeField] private string _action;

        private void Start()
        {
            UpdateSprite();
        }

        public void UpdateSprite()
        {
            if (_container.GetSprite(_action, out var sprite))
            {
                GetComponentInChildren<Image>().sprite = sprite;
                GetComponentInChildren<TextMeshProUGUI>().text = _action;
            }
        }
    }
}