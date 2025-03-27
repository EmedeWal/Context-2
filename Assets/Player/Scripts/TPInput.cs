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
        public bool SustainSprint;
        public bool Interact;
    }

    public class TPInput
    {
        public Quaternion RequestedRotation;
        public Vector3 RequestedMovement;
        public bool RequestedJump;
        public bool RequestedJumpCancel;
        public bool RequestedJumpSustain;
        public bool RequestedSustainedSprint;
        public bool RequestedInteract;

        public void UpdateInput(ControllerInput input)
        {
            // Movement
            RequestedMovement = new Vector3(input.Movement.x, 0f, input.Movement.y);
            RequestedMovement = input.Rotation * RequestedMovement;

            // Ensure there is movement before setting rotation
            if (RequestedMovement.sqrMagnitude > 0.001f)
                RequestedRotation = Quaternion.LookRotation(RequestedMovement.normalized);

            // OnJumped
            RequestedJump = input.Jump || RequestedJump;
            RequestedInteract = input.Interact || RequestedInteract;
            RequestedJumpCancel = input.CancelJump || RequestedJumpCancel;

            RequestedSustainedSprint = input.SustainSprint;
            RequestedJumpSustain = input.SustainJump;
        }
    }
}