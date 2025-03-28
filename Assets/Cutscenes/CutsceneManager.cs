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
        [SerializeField] private CinemachineCamera _mainCamera;
        [SerializeField] private GameObject _worldUI;

        [Header("Children")]
        [SerializeField] private PlayableDirector _openingCutscene;
        [SerializeField] private PlayableDirector _endingCutscene;
        [SerializeField] private MaterialSwapper _finalGolem;

        [Header("Settings")]
        [SerializeField] private float _lerpDuration;
        [SerializeField] private float _activationDelayUI;

        private StaticConnectionPoint[] _staticPoints;
        private InputActions _actions;


        private void Start()
        {
            Instance = this;

            _staticPoints = GameObject.FindObjectsByType<StaticConnectionPoint>(FindObjectsSortMode.None);
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
                {
                    foreach (var point in _staticPoints)
                        point.UpdateVisuals(false, 1);

                    StopAllCoroutines();
                    StartCoroutine(ColorCoroutine(1));

                    _mainCamera.Priority = 10;
                    _openingCutscene.Stop();
                }

                if (_endingCutscene.state is PlayState.Playing)
                {
                    _endingCutscene.Stop();
                    LoadMainMenu();
                }
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

            Invoke(nameof(EnableUI), _activationDelayUI);
        }

        private void EnableUI()
        {
            _worldUI.SetActive(true);
        }

        public void LerpColorToDead()
        {
            foreach (var point in _staticPoints)
                point.UpdateVisuals(false, _lerpDuration);

            StartCoroutine(ColorCoroutine(_lerpDuration));
        }

        private IEnumerator ColorCoroutine(float duration)
        {
            ConnectionManager connectionManager = ConnectionManager.Instance;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;

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