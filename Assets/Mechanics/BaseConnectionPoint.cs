namespace Context
{
    using UnityEngine;

    [RequireComponent(typeof(Collider))]
    public abstract class BaseConnectionPoint : MonoBehaviour
    {
        [field: SerializeField] public BaseConnectionPoint[] InitialConnections { get; private set; }
        [field: SerializeField] public int MaxConnections { get; private set; } = 2;

        public Collider Collider { get; protected set; }

        protected ConnectionManager _manager;

        public virtual void Init(ConnectionManager connectionManager)
        {
            _manager = connectionManager;
            Collider = GetComponent<Collider>();
        }
    }
}