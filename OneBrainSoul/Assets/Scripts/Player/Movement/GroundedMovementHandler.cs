using UnityEngine;

namespace Player.Movement
{
    public class GroundedMovementHandler : IMovementHandler
    {
        public static float runSpeed = 168f;
        public static float jumpStrength = 14f;
        public static float horizontalDrag = 15f;
        public static float bobbingStrength = 0.3f;

        // Timers are public in case another state wants to set them before switching
        public float jumpTimer = .3f;
        public float coyoteTimer = .65f;

        private float bobbingCycle = 0f;

        private Vector3 prevDirection = Vector3.zero;

        public void ResetValues()
        {
            jumpTimer = .3f;
            coyoteTimer = .65f;
            bobbingCycle = 0f;
            prevDirection = Vector3.zero;
        }

        public void Move(PlayerCharacterController player)
        {
            Transform orientation = player.GetOrientation();
            
            // Running around
            Vector3 direction = (orientation.right * player.GetXInput() + orientation.forward * player.GetYInput()).normalized;
            direction = Quaternion.FromToRotation(Vector3.up, player.GetGroundHit().normal) * direction;
            player.GetRigidbody().AddForce(direction * (runSpeed * player.GetMoveSpeedMultiplier()), ForceMode.Acceleration);

            // Add camera bobbing torque
            bobbingCycle += Time.fixedDeltaTime * player.GetRigidbody().velocity.magnitude / 2f;
            player.GetRigidbody().AddTorque(Quaternion.Euler(0, 90, 0) * player.GetRigidbody().velocity * bobbingStrength, ForceMode.Acceleration);
            float sidewaysBobbingMagnitude = Mathf.Sin(bobbingCycle) * player.GetRigidbody().velocity.magnitude * bobbingStrength / 8f;
            player.GetRigidbody().AddTorque(orientation.forward * sidewaysBobbingMagnitude, ForceMode.Acceleration);

            // Jump has a slight cooldown after landing
            if (jumpTimer > 0.0f) jumpTimer -= 0.1f;

            // Jump and switch state to airborne
            if (player.HasPressedJump() && jumpTimer <= 0f)
            {
                if(player.IsOnTheGround())
                {
                    Vector3 pos = player.transform.position;
                    pos.y += player.GetHoverHeight() - player.GetGroundHit().distance;
                    player.transform.position = pos;
                }

                Vector3 velocity = player.GetRigidbody().velocity;
                velocity.y = jumpStrength;
                player.GetRigidbody().velocity = velocity;
                player.ChangeMovementHandlerToAirborne();
                return;
            }

            // Coyote time allows jumps briefly after walking off a platform
            if (player.IsOnTheGround())
            {
                coyoteTimer = .65f;
            }
            else
            {
                coyoteTimer -= 0.07f;
                if (coyoteTimer <= 0f || Vector3.Angle(player.GetGroundHit().normal, Vector3.up) > 37.5f)
                {
                    player.ChangeMovementHandlerToAirborne();
                    return;
                }
            }

            // Drag, opposite to current horizontal velocity
            Vector3 horizontalVelocity = player.GetRigidbody().velocity;
            horizontalVelocity.y = 0;
            player.GetRigidbody().AddForce(-horizontalVelocity * horizontalDrag, ForceMode.Acceleration);

            prevDirection = direction;
        }
    }
}