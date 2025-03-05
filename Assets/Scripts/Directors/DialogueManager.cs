namespace Context
{
    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections;
    using Unity.VisualScripting;

    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [SerializeField] private GameObject _dialogueUI; // UI Panel
        [SerializeField] private Text _dialogueText; // UI Text
        [SerializeField] private float _textSpeed = 0.05f;

        private string[] _currentDialogue;
        private int _currentIndex;
        private bool _isTyping;
        private InputActions _inputActions;

        private void OnEnable()
        {
            Instance = this;

            _inputActions = InputManager.Instance.Actions;
        }

        private void OnDisable()
        {
            Instance = null;
        }

        private void Update()
        {
            var menuMap = _inputActions.Menu;
            if (menuMap.Continue.WasPressedThisFrame())
                NextDialogue();
        }

        public void StartDialogue(string[] dialogue)
        {
            if (dialogue == null || dialogue.Length == 0) return;

            _currentDialogue = dialogue;
            _currentIndex = 0;
            _dialogueUI.SetActive(true);
            InputManager.Instance.LockGameplayInput();

            ShowDialogue();
        }

        private void ShowDialogue()
        {
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

        public void NextDialogue()
        {
            if (_isTyping || _currentIndex >= _currentDialogue.Length) return;

            _currentIndex++;

            if (_currentIndex < _currentDialogue.Length)
            {
                ShowDialogue();
            }
            else
            {
                EndDialogue();
            }
        }

        private void EndDialogue()
        {
            _dialogueUI.SetActive(false);
            InputManager.Instance.UnlockGameplayInput();
        }
    }
}