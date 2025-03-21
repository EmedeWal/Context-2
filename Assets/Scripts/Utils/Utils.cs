namespace Context
{
    using UnityEngine;

    public static class Layers
    {
        private const string _controller = "Controller";

        public static LayerMask GetControllerLayer() => LayerMask.NameToLayer(_controller);
    }
}