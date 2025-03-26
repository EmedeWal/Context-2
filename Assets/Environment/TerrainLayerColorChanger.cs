namespace Context
{
    using UnityEngine;

    public class TerrainLayerColorChanger : MonoBehaviour
    {
        public static TerrainLayerColorChanger Instance { get; private set; }

        [SerializeField] private Color _aliveColor;
        [SerializeField] private Color _deadColor;

        private Terrain _terrain;
        private TerrainData _terrainData;

        private void Awake()
        {
            Instance = this;

            _terrain = GetComponent<Terrain>();
            _terrainData = _terrain.terrainData;
        }

        private void OnDisable()
        {
            Instance = null;    
        }

        public void ChangeLayerColor(int index, float finishedPercentage)
        {
            TerrainLayer layer = _terrainData.terrainLayers[index];
            layer.diffuseRemapMax = Color.Lerp(_deadColor, _aliveColor, finishedPercentage);
        }
    }
}