namespace Context
{
    using UnityEngine.Playables;
    using Unity.Cinemachine;
    using UnityEngine;
    using System.Collections;

    public class CutsceneManager : MonoBehaviour
    {
        public static CutsceneManager Instance { get; private set; }

        [Header("World")]
        [SerializeField] private StaticConnectionPoint[] _staticPoints;
        [SerializeField] private CinemachineCamera _mainCamera;

        [Header("Children")]
        [SerializeField] private PlayableDirector _openingCutscene;
        [SerializeField] private PlayableDirector _endingCutscene;
        [SerializeField] private MaterialSwapper _finalGolem;

        [Header("Settings")]
        [SerializeField] private float _lerpDuration;

        private InputActions _actions;


        private void Start()
        {
            Instance = this;

            _actions = ApplicationManager.Instance.InputManager.Actions;

            PlayCutscene(_openingCutscene);

            _openingCutscene.stopped += OpeningCutsceneStopped;
        }

        private void Update()
        {
            var menu = _actions.Menu;
            if (menu.Continue.WasPressedThisFrame())
            {
                if (_openingCutscene.state is PlayState.Playing)
                    _openingCutscene.Stop();

                if (_endingCutscene.state is PlayState.Playing)
                    _endingCutscene.Stop();
            }
        }

        private void OnDisable()
        {
            Instance = null;

            _openingCutscene.stopped -= OpeningCutsceneStopped;
        }

        private void PlayCutscene(PlayableDirector director)
        {
            _mainCamera.Priority = 0;

            director.Play();
        }

        private void OpeningCutsceneStopped(PlayableDirector director)
        {
            _mainCamera.Priority = 10;
        }

        public void LerpColorToDead()
        {
            foreach (var point in _staticPoints)
                point.UpdateVisuals(false, _lerpDuration);

            StartCoroutine(ColorCoroutine());
        }

        private IEnumerator ColorCoroutine()
        {
            ConnectionManager connectionManager = ConnectionManager.Instance;
            float elapsedTime = 0f;

            while (elapsedTime < _lerpDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / _lerpDuration;

                connectionManager.OverrideValue = Mathf.Lerp(1, 0, t);

                yield return null;
            }
        }

        public void PlayEndingCutscene()
        {
            _finalGolem.StartSwapping();
            PlayCutscene(_endingCutscene);
        }

        public void LoadMainMenu()
        {
            ApplicationManager.Instance.SceneLoader.TransitionToScene(0);
        }

        public bool IsPlayingCutscene() =>
            _openingCutscene.state is PlayState.Playing || _endingCutscene.state is PlayState.Playing;
    }
}