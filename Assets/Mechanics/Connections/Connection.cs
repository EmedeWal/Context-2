namespace Context
{
    using System;
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

        [Space]
        [Header("Spring")]
        [SerializeField] private AnimationCurve _springCurve;
        [SerializeField] private float _pointsPerUnit = 10f; // Determines resolution
        [SerializeField] private float _waveHeight = 0.5f;
        [SerializeField] private float _waveCount = 3f;
        [SerializeField] private float _velocity = 15f;

        private LineRenderer _line;
        private Mesh _bakedMesh;

        public void Init(BaseConnectionPoint pointA, BaseConnectionPoint pointB, bool stable)
        {
            _line = GetComponent<LineRenderer>();
            MeshCollider = GetComponent<MeshCollider>();

            _line.colorGradient = _stableDefaultGradient;
            _line.numCornerVertices = 6;
            _line.numCapVertices = 6;
            _line.startWidth = _sizeWidth;
            _line.endWidth = _sizeWidth;

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

        public void LateTick(Collider exceptionA, Collider exceptionB, float time)
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

            Obstructed = IsObstructed(colliderA, colliderB, pointA, pointB, exceptionA, exceptionB);
            _line.colorGradient = Obstructed
                ? obstructedGradient
                : defaultGradient;

            UpdateConnection(pointA, pointB, time);
        }

        private void UpdateRope(Vector3 pointA, Vector3 pointB, float time)
        {
            float noiseOffset = time * _velocity; // Move Perlin noise over time
            Vector3 direction = (pointB - pointA).normalized;

            Vector3 right = Vector3.Cross(direction, Vector3.up).normalized;
            Vector3 upDirection = Vector3.Cross(right, direction).normalized; // Correct up vector

            for (int i = 0; i < _line.positionCount; i++)
            {
                float delta = i / (float)(_line.positionCount - 1); // Proper interpolation
                Vector3 basePosition = Vector3.Lerp(pointA, pointB, delta);

                // Generate Perlin noise based on world position & time
                float noise = Mathf.PerlinNoise(delta * _waveCount, noiseOffset) * 2f - 1f; // Normalize to [-1, 1]
                float waveOffset = noise * _waveHeight;

                Vector3 finalPosition = basePosition + upDirection * waveOffset;
                _line.SetPosition(i, finalPosition);
            }
        }


        public void SetupConnection(BaseConnectionPoint a, BaseConnectionPoint b)
        {
            AttachedPoints = new BaseConnectionPoint[2] { a, b };
            UpdateConnection(AttachedPoints[0].Collider.bounds.center, AttachedPoints[1].Collider.bounds.center, Time.time);
        }

        public void UpdateConnection(Vector3 pointA, Vector3 pointB, float time)
        {
            // Determine the number of segments based on distance
            float distance = Vector3.Distance(pointA, pointB);
            var lineQuality = Mathf.Max(2, Mathf.RoundToInt(distance * _pointsPerUnit)); // Ensure at least 2 points

            _line.positionCount = lineQuality + 1; // Update line segment count

            UpdateRope(pointA, pointB, time);
            UpdateMeshCollider();
        }

        private bool IsObstructed(Collider colliderA, Collider colliderB, Vector3 pointA, Vector3 pointB, Collider exceptionA, Collider exceptionB)
        {
            // Offset the points slightly to avoid overlapping with colliders
            var start = colliderA.ClosestPoint(pointB);
            var end = colliderB.ClosestPoint(pointA);

            var dir = end - start; // DO THIS IN THIS ORDER
            var hits = Physics.SphereCastAll(start, _checkWidth, dir.normalized, dir.magnitude);

            // Exclude player
            hits = hits.Where(hit => hit.collider != null && !hit.collider.CompareTag("Player")).ToArray();
            return hits.Any(hit =>
                hit.collider != MeshCollider && // Exclude its own mesh collider
                hit.collider != colliderA &&    // Exclude its own connection point A
                hit.collider != colliderB &&   // Exclude its own connection point B
                hit.collider != exceptionA &&
                hit.collider != exceptionB // Exclude the player connections mesh collider
            );
        }

        private void UpdateLinePoints(Vector3 pointA, Vector3 pointB)
        {
            _line.SetPosition(0, pointA);
            _line.SetPosition(1, pointB);
        }

        private void UpdateMeshCollider()
        {
            if (_bakedMesh != null) Destroy(_bakedMesh); // Destroy previous mesh to free memory

            _bakedMesh = new Mesh();
            _line.BakeMesh(_bakedMesh, true);
            MeshCollider.sharedMesh = _bakedMesh;

            MeshCollider.convex = true;
            MeshCollider.isTrigger = true;
        }
    }
}