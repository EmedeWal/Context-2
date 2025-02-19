namespace Context
{
    using UnityEngine;

    [RequireComponent(typeof(Collider))]
    public class ConnectionPoint : MonoBehaviour
    {
        [SerializeField] private Collider[] _connectionPoints;

        private void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("Connectable");

            var instance = ConnectionManager.Instance;
            var collider = GetComponent<Collider>();

            foreach (var connection in _connectionPoints)
                if (connection != null)
                    instance.CreateConnection(collider, connection);
        }
    }
}