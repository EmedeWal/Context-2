namespace Context.ThirdPersonController
{
    using UnityEngine;

    public struct ControllerInput
    {
        public Quaternion Rotation;
        public Vector3 Movement;
        public bool Jump;
        public bool CancelJump;
        public bool SustainJump;
    }

    public class TPInput
    {
        public Quaternion RequestedRotation;
        public Vector3 RequestedMovement;
        public float TimeSinceJumpRequest;
        public bool RequestedJump;
        public bool RequestedJumpCancel;
        public bool RequestedJumpSustain;

        public void UpdateInput(ControllerInput input, Quaternion transientRotation)
        {
            // Movement
            RequestedMovement = new Vector3(input.Movement.x, 0f, input.Movement.y);
            RequestedMovement = Vector3.ClampMagnitude(RequestedMovement, 1f);
            RequestedMovement = input.Rotation * RequestedMovement;

            RequestedRotation = RequestedMovement.sqrMagnitude > 0
                ? Quaternion.LookRotation(RequestedMovement)
                : transientRotation;

            // Jump
            RequestedJump = input.Jump || RequestedJump;
            RequestedJumpCancel = input.CancelJump || RequestedJumpCancel;
            RequestedJumpSustain = input.SustainJump;
        }
    }
}