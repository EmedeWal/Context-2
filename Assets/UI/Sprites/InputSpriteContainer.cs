namespace Context
{
    using UnityEngine;
    using System.Collections.Generic;

    [CreateAssetMenu(fileName = "Input Sprite Container", menuName = "Scriptable Objects/Input Sprite Container")]
    public class InputSpriteContainer : ScriptableObject
    {
        [System.Serializable]
        public class ActionSprites
        {
            public string ActionName;
            [SerializeField] private string _binding;
            public Sprite[] Sprites = new Sprite[3];
        }

        [SerializeField] private List<ActionSprites> _actionSpritesList;

        public Sprite GetSprite(string actionName)
        {
            var inputType = InputManager.Instance.GetInputType();

            foreach (var actionSprites in _actionSpritesList)
                if (actionSprites.ActionName == actionName)
                    return actionSprites.Sprites[(int)inputType];

            Debug.LogWarning($"No sprite found for action: {actionName}");
            return null;
        }
    }
}