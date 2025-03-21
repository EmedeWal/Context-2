namespace Context
{
    using UnityEngine.SceneManagement;
    using UnityEngine;

    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance;

        [SerializeField] private GameObject _canvasObject;

        private Overlay _overlay;

        private const float FADE_TIME = 0.5f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                DestroyImmediate(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            var canvasObject = Instantiate(_canvasObject, transform);
            _overlay = new(canvasObject);
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            _overlay.Tick(deltaTime);
        }

        public int GetCurrentBuildIndex() => 
            SceneManager.GetActiveScene().buildIndex;

        public void TransitionToScene(int buildIndex)
        {
            _overlay.FadeRoutineOverlay
            (
                fadeHaltAction: () => SceneManager.LoadScene(buildIndex),
                fadeCompleteAction: null,
                overlayColor: Color.black,
                fadeTime: FADE_TIME
            );
        }

        public string GetCurrentSceneName() =>
            SceneManager.GetActiveScene().name;

        public void TransitionToScene(string sceneName)
        {
            _overlay.FadeRoutineOverlay
            (
                fadeHaltAction: () => SceneManager.LoadScene(sceneName),
                fadeCompleteAction: null,
                overlayColor: Color.black,
                fadeTime: FADE_TIME
            );
        }
    }
}