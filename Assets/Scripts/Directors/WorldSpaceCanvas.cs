namespace Context
{
    using UnityEngine;
    using UnityEngine.UI;

    public class WorldSpaceCanvas : MonoBehaviour
    {
        public static WorldSpaceCanvas Instance { get; private set; }

        [SerializeField] private InputSpriteContainer _container;
        [SerializeField] private Image _promptImage;

        public bool IsEnabled => _promptImage.enabled;

        private void OnEnable()
        {
            Instance = this;

            HidePrompt();
        }

        private void OnDisable()
        {
            Instance = null;
        }

        public void ShowPrompt(Vector3 position, float yOffset, string action)
        {
            position.y += yOffset;

            _promptImage.sprite = _container.GetSprite(action);
            _promptImage.transform.position = position;
            _promptImage.enabled = true;
        }

        public void HidePrompt()
        {
            _promptImage.enabled = false;
        }
    }
}