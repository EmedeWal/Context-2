namespace Context.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Image))]
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
                GetComponent<Image>().sprite = sprite;
        }
    }
}