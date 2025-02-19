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

        public void LateTick()
        {
            var a = Colliders[0].bounds.center;
            var b = Colliders[1].bounds.center;

            var dir = b - a;
            var ray = new Ray(a, dir);

            var obstructed = Physics.Raycast(ray, out var hit, dir.magnitude)
                          && !Colliders.Contains(hit.collider);

            //                           && (!Colliders.Contains(hit.collider) || hit.collider != _meshCollider);

            _lineRenderer.colorGradient = obstructed 
                ? _obstructedGradient 
                : _defaultGradient;

            UpdateLinePoints(a, b);
            UpdateMeshCollider(); // Update the collider mesh
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
            //Mesh mesh = new();
            //_lineRenderer.BakeMesh(mesh, true);
            //_meshCollider.sharedMesh = mesh;
            //_meshCollider.convex = true; // Needed for trigger colliders
            //_meshCollider.isTrigger = true; // Set to trigger if needed
        }
    }
}