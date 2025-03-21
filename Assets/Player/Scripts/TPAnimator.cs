namespace Context.ThirdPersonController
{
    using UnityEngine;
    using System;

    [RequireComponent(typeof(Animator))]
    public class TPAnimator : MonoBehaviour
    {
        public static event Action Footstep;

        public void OnFootstep() =>
            Footstep?.Invoke();
    }
}