namespace Context
{
    using UnityEngine;

    public class StaticConnectionPoint : BaseConnectionPoint
    {
        public override void Init(ConnectionManager connectionManager)
        {
            base.Init(connectionManager);

            gameObject.layer = LayerMask.NameToLayer("Interactable");
        }
    }
}