namespace Context
{
    using System;
    using UnityEngine;

    public class TerrainLayerColorChanger : MonoBehaviour
    {
        public static TerrainLayerColorChanger Instance { get; private set; }

        [SerializeField] private Color _aliveColor;
        [SerializeField] private Color _deadColor;
        [SerializeField] private float _transitionSpeed = 5f;

        private Terrain _terrain;
        private TerrainData _terrainData;
        private Color[] _targetColors;

        private void Awake()
        {
            Instance = this;

            _terrain = GetComponent<Terrain>();
            _terrainData = _terrain.terrainData;
        }

        private void Update()
        {
            var response = 1f - Mathf.Exp(-_transitionSpeed * Time.deltaTime);
           
            for (int i = 0;  i < _targetColors.Length; i++)
            {
                var layer = _terrainData.terrainLayers[i];
                layer.diffuseRemapMax = Color.Lerp(layer.diffuseRemapMax, _targetColors[i], response);

                // Reassign the layers (this forces an update)
                TerrainLayer[] newLayers = _terrainData.terrainLayers;
                newLayers[i] = layer;
                _terrainData.terrainLayers = newLayers;
            }
        }

        private void OnDisable()
        {
            Instance = null;    
        }

        public void ChangeLayerColor(int index, float finishedPercentage)
        {
            _targetColors[index] = Color.Lerp(_deadColor, _aliveColor, finishedPercentage);
        }
    }
}