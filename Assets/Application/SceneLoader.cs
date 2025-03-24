namespace Context
{
    using UnityEngine.SceneManagement;
    using UnityEngine;

    public class SceneLoader
    {
        private readonly Overlay _overlay;

        private const float FADE_TIME = 0.5f;

        public SceneLoader(Overlay overlay)
        {
            _overlay = overlay;
        }

        public void Tick(float deltaTime)
        {
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
    }
}