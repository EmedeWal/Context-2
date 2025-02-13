namespace Context
{
    using UnityEngine;
    using TMPro;

    public class BuildLogger : MonoBehaviour
    {
        public static BuildLogger Instance;

        [SerializeField] private TextMeshProUGUI[] _speedArray;

        private void Awake()
        {
            Instance = this;

            transform.GetChild(0).gameObject.SetActive(true);
        }

        public void LogSpeed(float horizontalSpeed, float verticalSpeed)
        {
            var roundedHorizontal = Mathf.RoundToInt(horizontalSpeed);
            var roundedVertical = Mathf.RoundToInt(verticalSpeed);

            _speedArray[0].text = $"Horiz: {roundedHorizontal}";
            _speedArray[1].text = $"Vert: {roundedVertical}";
        }
    }
}