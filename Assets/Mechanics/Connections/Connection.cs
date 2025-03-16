namespace Context
{
    using System.Linq;
    using UnityEngine;

    [RequireComponent(typeof(LineRenderer), typeof(MeshCollider))]
    public class Connection : MonoBehaviour
    {
        public BaseConnectionPoint[] AttachedPoints { get; private set; }
        public MeshCollider MeshCollider {  get; private set; }
        public bool Obstructed { get; private set; }

        public bool Stable;

        [Header("SETTINGS")]

        [Space]
        [Header("Colors")]
        [SerializeField] private Gradient _unstableObstructedGradient;
        [SerializeField] private Gradient _unstableDefaultGradient;
        [SerializeField] private Gradient _stableObstructedGradient;
        [SerializeField] private Gradient _stableDefaultGradient;

        [Space]
        [Header("Size")]
        [SerializeField] private float _sizeWidth = 0.15f;
        [SerializeField] private float _checkWidth = 0.03f;

        private LineRenderer _lineRenderer;
        private Mesh _bakedMesh;

        public void Init(BaseConnectionPoint pointA, BaseConnectionPoint pointB, bool stable)
        {
            _lineRenderer = GetComponent<LineRenderer>();
            MeshCollider = GetComponent<MeshCollider>();

            _lineRenderer.colorGradient = _stableDefaultGradient;
            _lineRenderer.numCornerVertices = 6;
            _lineRenderer.numCapVertices = 6;
            _lineRenderer.positionCount = 2;
            _lineRenderer.startWidth = _sizeWidth;
            _lineRenderer.endWidth = _sizeWidth;

            Stable = stable;

            SetupConnection(pointA, pointB);
        }

        public void Cleanup()
        {
            if (MeshCollider != null && MeshCollider.sharedMesh != null)
            {
                Destroy(MeshCollider.sharedMesh);
                MeshCollider.sharedMesh = null;
            }

            if (_bakedMesh != null)
            {
                Destroy(_bakedMesh);
                _bakedMesh = null;
            }
        }

        public void LateTick(Collider collider)
        {
            var colliderA = AttachedPoints[0].Collider;
            var colliderB = AttachedPoints[1].Collider;
            var pointA = colliderA.bounds.center;
            var pointB = colliderB.bounds.center;

            var obstructedGradient = Stable
                ? _stableObstructedGradient
                : _unstableObstructedGradient;

            var defaultGradient = Stable
                ? _stableDefaultGradient
                : _unstableDefaultGradient;  

            Obstructed = IsObstructed(colliderA, colliderB, pointA, pointB, collider);
            _lineRenderer.colorGradient = Obstructed
                ? obstructedGradient
                : defaultGradient;

            UpdateConnection(pointA, pointB);
        }

        public void SetupConnection(BaseConnectionPoint a, BaseConnectionPoint b)
        {
            AttachedPoints = new BaseConnectionPoint[2] { a, b };
            UpdateConnection(AttachedPoints[0].Collider.bounds.center, AttachedPoints[1].Collider.bounds.center);
        }

        public void UpdateConnection(Vector3 pointA, Vector3 pointB)
        {
            UpdateLinePoints(pointA, pointB);
            UpdateMeshCollider();
        }

        private bool IsObstructed(Collider colliderA, Collider colliderB, Vector3 pointA, Vector3 pointB, Collider collider)
        {
            // Offset the points slightly to avoid overlapping with colliders
            var start = colliderA.ClosestPoint(pointB);
            var end = colliderB.ClosestPoint(pointA);

            var dir = end - start; // DO THIS IN THIS ORDER
            var hits = Physics.SphereCastAll(start, _checkWidth, dir.normalized, dir.magnitude);

            // Exclude player
            hits = hits.Where(hit => hit.collider != null && !hit.collider.CompareTag("Player")).ToArray();
            Debug.Log(hits.Length);
            return hits.Any(hit =>
                hit.collider != MeshCollider && // Exclude its own mesh collider
                hit.collider != colliderA &&    // Exclude its own connection point A
                hit.collider != colliderB &&   // Exclude its own connection point B
                hit.collider != collider      // Exclude the player connections mesh collider
            );
        }

        private void UpdateLinePoints(Vector3 pointA, Vector3 pointB)
        {
            _lineRenderer.SetPosition(0, pointA);
            _lineRenderer.SetPosition(1, pointB);
        }

        private void UpdateMeshCollider()
        {
            if (_bakedMesh != null) Destroy(_bakedMesh); // Destroy previous mesh to free memory

            _bakedMesh = new Mesh();
            _lineRenderer.BakeMesh(_bakedMesh, true);
            MeshCollider.sharedMesh = _bakedMesh;

            MeshCollider.convex = true;
            MeshCollider.isTrigger = true;
        }
    }
}