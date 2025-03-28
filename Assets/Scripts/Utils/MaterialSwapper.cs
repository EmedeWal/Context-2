using UnityEngine;

namespace Context
{
    public class MaterialSwapper : MonoBehaviour
    {
        [Header("REFERENCES")]
        [SerializeField] private Material _startMaterial;

        [Header("Materials")]
        [SerializeField] private Material _defaultMaterial; // Dark Material
        [SerializeField] private Material _litMaterial; // Light Material

        [Header("Hierarchy")]
        [SerializeField] private MeshRenderer[] _meshRenderers;

        [Header("SETTINGS")]
        [SerializeField] private float _time = 1;
        [SerializeField] private float _repeatRate = 1; 

        private bool _isLit; // Tracks current material state

        private void Start()
        {
            SetMaterials(_startMaterial);
        }

        public void StartSwapping()
        {
            InvokeRepeating(nameof(SwapMaterials), _time, _repeatRate);
        }

        public void StopSwapping()
        {
            CancelInvoke(nameof(SwapMaterials));
            SetMaterials(_defaultMaterial); // Reset to dark material
            _isLit = false; // Ensure state is reset
        }

        private void SwapMaterials()
        {
            _isLit = !_isLit;
            SetMaterials(_isLit ? _litMaterial : _defaultMaterial);
        }

        private void SetMaterials(Material material)
        {
            foreach (var renderer in _meshRenderers)
                renderer.material = material;
        }

        // New Functions to Manually Set Materials
        public void SetDarkMaterial()
        {
            SetMaterials(_defaultMaterial); // Set to dark material
            _isLit = false; // Ensure the state reflects dark material
        }

        public void SetLightMaterial()
        {
            SetMaterials(_litMaterial); // Set to light material
            _isLit = true; // Ensure the state reflects light material
        }
    }
}
