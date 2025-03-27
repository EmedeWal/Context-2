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

        [Header("Settings")]
        [SerializeField] private bool _shouldSwap = true; // Control swapping per object

        private bool _isLit; // Tracks current material state

        private void Start()
        {
            SetMaterials(_startMaterial);

            if (_shouldSwap) // Only start swapping if enabled
                StartSwapping();
        }

        public void StartSwapping()
        {
            if (!_shouldSwap) return; // Prevents starting if disabled
            InvokeRepeating(nameof(SwapMaterials), 1f, 1f);
        }

        public void StopSwapping()
        {
            CancelInvoke(nameof(SwapMaterials));
            SetMaterials(_defaultMaterial); // Reset to dark material
            _isLit = false; // Ensure state is reset
        }

        private void SwapMaterials()
        {
            if (!_shouldSwap) return; // If swapping is disabled, do nothing
            _isLit = !_isLit;
            SetMaterials(_isLit ? _litMaterial : _defaultMaterial);
        }

        private void SetMaterials(Material material)
        {
            foreach (var renderer in _meshRenderers)
                renderer.material = material;
        }

        public void ToggleSwapping(bool enable)
        {
            _shouldSwap = enable;

            if (_shouldSwap)
                StartSwapping();
            else
                StopSwapping();
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
