using Managers;
using UnityEngine;

namespace Player.Movement
{
    public class WallClimbMovementHandler : IMovementHandler
    {
        public static float runSpeed = 450f;
        public static float strafeSpeed = 30f;
        public static float torqueStrength = 7f;
        public static float jumpStrength = 14f;
        public static float verticalDrag = 30f;
        public static float horizontalDrag = 7f;
        public static float bobbingStrength = 5f;
        public static float bonusSpeedDuration = .2f;
        public static float bonusSpeed = 180f;

        public static float viewAngleThreshold = 0.85f;
        public static float moveAngleThreshold = 0.4f;
        public static float climbDuration = 1f;

        private float climbTime = 0f;
        private float bobbingCycle = 0f;

        public void ResetValues()
        {
            climbTime = 0;
            bobbingCycle = 0;
        }

        public void Move(PlayerCharacterController player)
        {
            Rigidbody rigidbody = player.GetRigidbody();
            Transform orientation = player.GetOrientation();
            
            float additionalSpeed = Mathf.Lerp(bonusSpeed, 0f, Mathf.Clamp01(climbTime / bonusSpeedDuration));
            rigidbody.AddForce(Vector3.up * (runSpeed + additionalSpeed), ForceMode.Acceleration);

            Vector3 direction = (orientation.right * player.GetXInput() + orientation.forward * player.GetYInput()).normalized;
            Vector3 directionStrafe = (orientation.right * player.GetXInput()).normalized;

            rigidbody.AddForce(-new Vector3(0f, rigidbody.velocity.y, 0f) * verticalDrag, ForceMode.Acceleration);
            rigidbody.AddForce(-new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z) * horizontalDrag, ForceMode.Acceleration);

            rigidbody.AddForce(directionStrafe * (strafeSpeed * player.GetMoveSpeedMultiplier()), ForceMode.Acceleration);
        
            Vector3 horizontalVelocity = rigidbody.velocity;
            horizontalVelocity.y = 0;
            rigidbody.AddTorque(Quaternion.Euler(0, 90, 0) * horizontalVelocity * torqueStrength, ForceMode.Acceleration);

            bobbingCycle += Time.fixedDeltaTime * rigidbody.velocity.magnitude * 0.75f;
            float sidewaysBobbingMagnitude = Mathf.Sin(bobbingCycle) * rigidbody.velocity.magnitude * bobbingStrength / 8f;
            rigidbody.AddTorque(orientation.forward * sidewaysBobbingMagnitude, ForceMode.Acceleration);

            if (!player.HasPressedJump())
            {
                player.ChangeMovementHandlerToAirborne();
                return;
            }

            RaycastHit hit;
            if (Physics.Raycast(player.transform.position + new Vector3(0f, 1.9f, 0f), orientation.forward, out hit, 1f, GameManager.Instance.GetRaycastLayersWithoutAlly(), QueryTriggerInteraction.Ignore))
            {
                float viewDot = Vector3.Dot(hit.normal, orientation.forward);
                float moveDot = Vector3.Dot(hit.normal, orientation.right * player.GetXInput() + orientation.forward * player.GetYInput());
                if (viewDot >= -viewAngleThreshold || moveDot >= -moveAngleThreshold)
                {
                    player.ChangeMovementHandlerToAirborne();
                }
            }
            else
            {
                player.ChangeMovementHandlerToAirborne();
            }

            if (climbTime > climbDuration)
            {
                player.ChangeMovementHandlerToAirborne();
            }

            climbTime += Time.fixedDeltaTime;
        }

        public bool ShouldGravityApply(PlayerCharacterController player)
        {
            return false;
        }

        public bool ShouldHoverApply(PlayerCharacterController player)
        {
            return true;
        }
    }
}
