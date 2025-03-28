namespace Context
{
    using TMPro;
    using UnityEngine;
    using System.Collections;
    using UnityEngine.Rendering;

    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        public bool ActiveDialogue => _holder.activeSelf;

        [SerializeField] private GameObject _holder; // UI Panel
        [SerializeField] private TextMeshProUGUI _dialogueText; // UI Text
        [SerializeField] private float _textSpeed = 0.05f;

        private InputActions _inputActions;
        private string[] _currentDialogue;
        private int _currentIndex;
        private bool _isTyping;

        private void Start()
        {
            Instance = this;

            _inputActions = ApplicationManager.Instance.InputManager.Actions;

            transform.GetChild(0).gameObject.SetActive(true);
            SetHolderActive(false);
        }

        private void OnDisable()
        {
            Instance = null;
        }

        private void Update()
        {
            if (!ActiveDialogue) return;

            //var menuMap = _inputActions.Menu;
            //if (menuMap.Continue.WasPressedThisFrame())
            //{
            //    if (_isTyping)
            //    {
            //        StopAllCoroutines();
            //        _dialogueText.text = _currentDialogue[_currentIndex]; // Instantly set full text
            //        _isTyping = false;
            //    }
            //    else
            //    {
            //        NextDialogue();
            //    }
            //}

           
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.anyKeyDown)
            {
                if (_isTyping)
                {
                    StopAllCoroutines();
                    _dialogueText.text = _currentDialogue[_currentIndex]; // Instantly set full text
                    _isTyping = false;
                }
                else
                {
                    NextDialogue();
                }
            }
        }

        public void StartDialogue(string[] dialogue)
        {
            if (dialogue == null || dialogue.Length == 0) return;

            ApplicationManager.Instance.InputManager.LockGameplayInput();
            _currentDialogue = dialogue;
            _currentIndex = 0;

            ShowDialogue();
        }

        private void ShowDialogue()
        {
            SetHolderActive(true);
            _dialogueText.text = "";
            StartCoroutine(TypeText(_currentDialogue[_currentIndex]));
        }

        private IEnumerator TypeText(string text)
        {
            _isTyping = true;
            _dialogueText.text = "";

            foreach (char letter in text.ToCharArray())
            {
                _dialogueText.text += letter;
                yield return new WaitForSeconds(_textSpeed);
            }

            _isTyping = false;
        }

        private void NextDialogue()
        {
            if (_isTyping || _currentIndex >= _currentDialogue.Length) return;

            _currentIndex++;

            if (_currentIndex < _currentDialogue.Length) ShowDialogue();
            else EndDialogue();
        }

        private void EndDialogue()
        {
            SetHolderActive(false);
            ApplicationManager.Instance.InputManager.UnlockGameplayInput();
        }

        private void SetHolderActive(bool active)
        {
            _holder.SetActive(active);
        }
    }
}