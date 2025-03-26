namespace Context.UI
{
    using UnityEngine;
    using UnityEngine.Video;

    [RequireComponent(typeof(VideoPlayer))]
    public class CreditController : MonoBehaviour
    {
        private InputActions _actions;
        private VideoPlayer _player;

        private void Start()
        {
            _player = GetComponent<VideoPlayer>();
            
            _player.Play();
            _player.loopPointReached += OnVideoFinished;

            _actions = ApplicationManager.Instance.InputManager.Actions;
        }

        private void Update()
        {
            var menu = _actions.Menu;
            if (menu.Continue.WasPressedThisFrame())
                OnVideoFinished(_player);
        }

        private void OnVideoFinished(VideoPlayer videoPlayer)
        {
            ApplicationManager.Instance.SceneLoader.TransitionToScene(0);
        }
    }
}