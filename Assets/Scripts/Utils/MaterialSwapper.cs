namespace Context
{
    using UnityEngine;

    public class MaterialSwapper : MonoBehaviour
    {
        [Header("REFERENCES")]

        [Space]
        [Header("Activation")]
        [SerializeField] private Material _startMaterial;

        [Space]
        [Header("Materials")]
        [SerializeField] private Material _defaultMaterial;
        [SerializeField] private Material _litMaterial;

        [Space]
        [Header("Hierarchy")]
        [SerializeField] private SkinnedMeshRenderer _bodyMeshRenderer;
        [SerializeField] private MeshRenderer[] _meshRenderers;

        private void Start()
        {
            SetMaterials(_startMaterial);
        }

        public void SetDefaultMaterials() =>
            SetMaterials(_defaultMaterial);

        public void SetLitMaterials() =>
            SetMaterials(_litMaterial);

        private void SetMaterials(Material material)
        {
            if (_bodyMeshRenderer != null)
                _bodyMeshRenderer.material = material;

            foreach (var renderer in _meshRenderers)
                renderer.material = material;
        }
    }
}