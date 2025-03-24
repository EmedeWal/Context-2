namespace Context
{
    using UnityEngine;

    public class StaticConnectionPoint : BaseConnectionPoint
    {
        [Header("REFERENCES")]

        [Space]
        [Header("Materials")]
        [SerializeField] private Material _defaultMaterial;
        [SerializeField] private Material _litMaterial;

        [Space]
        [Header("Hierarchy")]
        [SerializeField] private SkinnedMeshRenderer _bodyMeshRenderer;
        [SerializeField] private MeshRenderer[] _meshRenderers;

        public override void Init(ConnectionManager connectionManager)
        {
            base.Init(connectionManager);

            gameObject.layer = LayerMask.NameToLayer("Interactable");

            SetMaterials(_defaultMaterial);
        }

        public void StartConnection(Vector3 callerPosition)
        {
            var animator = GetComponentInChildren<Animator>();
            if (animator == null) return;
            
            animator.CrossFade("Connect", 0.1f);
            var direction = transform.position - callerPosition;
            direction = Vector3.ProjectOnPlane(direction, transform.up);

            if (direction.sqrMagnitude > 0.1f)
                transform.rotation = Quaternion.LookRotation(direction, transform.up);
        }

        protected override void OnCompletedConnections()
        {
            base.OnCompletedConnections();

            SetMaterials(_litMaterial);
        }

        protected override void OnIncompletedConnections()
        {
            SetMaterials(_defaultMaterial);
        }

        private void SetMaterials(Material material)
        {
            _bodyMeshRenderer.material = material;
            foreach (var renderer in _meshRenderers)
                renderer.material = material;
        }
    }
}