namespace Context
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "Dialogue Data", menuName = "Scriptable Objects/Dialogue Data")]
    public class DialogueDataSO : ScriptableObject
    {
        [field: SerializeField] public string[] Dialogue;
    }
}