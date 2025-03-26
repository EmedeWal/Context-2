namespace Context
{
    using UnityEngine;

    public class StaticConnectionPoint : BaseConnectionPoint
    {
        [Header("REFERENCES")]

        [Space]
        [Header("Behaviour")]
        [SerializeField] private bool _isPillar;

        [Space]
        [Header("Materials")]
        [SerializeField] private Material _defaultMaterial;
        [SerializeField] private Material _litMaterial;

        [Space]
        [Header("Environment")]
        [SerializeField] private Organism[] _organisms;

        [Space]
        [Header("Hierarchy")]
        [SerializeField] private SkinnedMeshRenderer _bodyMeshRenderer;
        [SerializeField] private MeshRenderer[] _meshRenderers;

        [field: SerializeField] public int LevelIndex { get; private set; }

        public override void Init(ConnectionManager connectionManager)
        {
            base.Init(connectionManager);

            gameObject.layer = LayerMask.NameToLayer("Interactable");

            UpdateVisuals(false, 0f);
        }

        public void StartConnection(Vector3 callerPosition)
        {
            if (_isPillar) return;

            var animator = GetComponentInChildren<Animator>();

            animator.CrossFade("Connect", 0.1f);
            var direction = transform.position - callerPosition;
            direction = Vector3.ProjectOnPlane(direction, transform.up);

            if (direction.sqrMagnitude > 0.1f)
                transform.rotation = Quaternion.LookRotation(direction, transform.up);
        }

        protected override void OnCompletedConnections()
        {
            base.OnCompletedConnections();

            UpdateVisuals(true);
        }

        protected override void OnIncompletedConnections()
        {
            UpdateVisuals(false);
        }

        private void UpdateVisuals(bool alive, float duration = 1f)
        {
            // This point
            var material = alive ? _litMaterial : _defaultMaterial;
            
            if (_bodyMeshRenderer != null)
                _bodyMeshRenderer.material = material;
                
            foreach (var renderer in _meshRenderers)
                renderer.material = material;

            // Environment
            foreach (var organism in _organisms)
                organism.SetState(alive, duration);
        }
    }
}