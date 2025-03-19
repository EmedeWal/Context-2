namespace Context
{
    using TMPro;
    using UnityEngine;
    using System.Collections;

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

        private void OnEnable()
        {
            Instance = this;

            _inputActions = InputManager.Instance.Actions;

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

            var menuMap = _inputActions.Menu;
            if (menuMap.Continue.WasPressedThisFrame())
                NextDialogue();
        }

        public void StartDialogue(string[] dialogue)
        {
            if (dialogue == null || dialogue.Length == 0) return;

            InputManager.Instance.LockGameplayInput();
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
                yield return new WaitForSecondsRealtime(_textSpeed);
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
            InputManager.Instance.UnlockGameplayInput();
        }

        private void SetHolderActive(bool active)
        {
            _holder.SetActive(active);
        }
    }
}