namespace Context
{
    using System.Linq;
    using UnityEngine;

    [RequireComponent(typeof(LineRenderer), typeof(MeshCollider))]
    public class Connection : MonoBehaviour
    {
        public Collider[] Colliders { get; private set; }

        [Header("SETTINGS")]

        [Space]
        [Header("Colors")]
        [SerializeField] private Gradient _defaultGradient;
        [SerializeField] private Gradient _obstructedGradient;

        [Space]
        [Header("Size")]
        [SerializeField] private float _width = 0.15f;

        private LineRenderer _lineRenderer;
        private MeshCollider _meshCollider;

        public void Init(Collider a, Collider b)
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _meshCollider = GetComponent<MeshCollider>();
            Colliders = new Collider[2] { a, b };

            if (a == b)
            {
                Debug.LogError("Should not assign a connection with two of the same colliders!");
                Destroy(gameObject);
            }
            else
            {
                _lineRenderer.numCornerVertices = 6;
                _lineRenderer.numCapVertices = 6;
                _lineRenderer.positionCount = 2;
                _lineRenderer.startWidth = _width;
                _lineRenderer.endWidth = _width;

                UpdateLinePoints(Colliders[0].bounds.center, Colliders[1].bounds.center);
                _lineRenderer.colorGradient = _defaultGradient;

                // Generate the collider mesh
                UpdateMeshCollider();
            }
        }

        public void FixedTick()
        {
            var a = Colliders[0].bounds.center;
            var b = Colliders[1].bounds.center;

            // Offset the points slightly to avoid overlapping with colliders
            var start = Colliders[0].ClosestPoint(b);
            var end = Colliders[1].ClosestPoint(a);

            var dis = Vector3.Distance(start, end);
            var radius = 0.01f;
            var dir = b - a;

            var hits = Physics.SphereCastAll(start, radius, dir, dis);

            var obstructed = hits.Any(hit =>
                hit.collider != null
                && hit.collider != _meshCollider
                && !Colliders.Contains(hit.collider)); // Ignore its own mesh collider

            _lineRenderer.colorGradient = obstructed
                ? _obstructedGradient
                : _defaultGradient;

            UpdateLinePoints(start, end);
            UpdateMeshCollider();
        }

        public void UpdateConnection(Collider collider, int index)
        {
            Colliders[index] = collider;
            UpdateMeshCollider();
        }

        private void UpdateLinePoints(Vector3 a, Vector3 b)
        {
            _lineRenderer.SetPosition(0, a);
            _lineRenderer.SetPosition(1, b);
        }

        private void UpdateMeshCollider()
        {
            Mesh mesh = new();
            _lineRenderer.BakeMesh(mesh, true);
            _meshCollider.sharedMesh = mesh;
            _meshCollider.convex = true; // Needed for trigger colliders
            _meshCollider.isTrigger = true; // Set to trigger if needed
        }
    }
}