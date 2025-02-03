using Managers;
using UnityEngine;

namespace Player.Movement
{
    public class AirborneMovementHandler : IMovementHandler
    {
        public static float strafeSpeed = 30f;
        public static float torqueStrength = 0.6f;
        public float horizontalDrag = 2.85f;
        public static float slideSpeed = 50f;
        public static float slideDistance = 2.5f;
        public static float slideAngle = 37.5f;

        public static float jumpStrength = 18f;
        public static float jumpForwardStrength = 11f;

        private float groundedTimer = 0f;
        public float jumpTimer = 0.6f;

        public float maxVelocity = 30f;

        int doubleJumps = 0;
        bool canDoubleJump = false;

        public void ResetValues()
        {
            groundedTimer = 0f;
            jumpTimer = 0.6f;
            doubleJumps = 0;
            canDoubleJump = false;
        }

        public void Move(PlayerCharacterController player)
        {
            Transform orientation = player.GetOrientation();
            
            // Mid-air strafe, with less control than grounded movement
            Vector3 direction = (orientation.right * player.GetXInput() + 
                                 orientation.forward * player.GetYInput()).normalized;

            RaycastHit hit;
            CapsuleCollider playerCollider = player.GetCapsuleCollider();
            Vector3 p1 = player.transform.position + playerCollider.center + Vector3.up * (-playerCollider.height * 0.5f);
            Vector3 p2 = p1 + Vector3.up * playerCollider.height;
            if (Physics.CheckCapsule(p1, p2, playerCollider.radius + 0.075f, GameManager.Instance.GetRaycastLayersWithoutAlly(), QueryTriggerInteraction.Ignore) && 
                Physics.CapsuleCast(p1, p2, playerCollider.radius, direction, out hit, playerCollider.radius + 1f, GameManager.Instance.GetRaycastLayers(), QueryTriggerInteraction.Ignore))
            {
                Vector3 newDir = (direction + new Vector3(hit.normal.x, 0f, hit.normal.z)).normalized;
                newDir *= Vector3.Dot(direction, newDir);
                direction = newDir;
            }

            player.GetRigidbody().AddForce(direction * (strafeSpeed * player.GetMoveSpeedMultiplier()), ForceMode.Acceleration);

            // Lean in the direction we are moving
            Vector3 horizontalVelocity = player.GetRigidbody().velocity;
            horizontalVelocity.y = 0;
            player.GetRigidbody().AddTorque(Quaternion.Euler(0, 90, 0) * horizontalVelocity * torqueStrength, ForceMode.Acceleration);
            player.GetRigidbody().AddForce(-horizontalVelocity * horizontalDrag, ForceMode.Acceleration);

            // If we aren't moving upwards and hit the ground, we are grounded
            if (player.IsOnTheGround())
            {
                groundedTimer += 0.1f;
                if (player.GetRigidbody().velocity.y < -player.GetGravityStrength() / 2f + 0.01f || 
                    groundedTimer > 0.1f && player.GetRigidbody().velocity.y < 0.01f || 
                    groundedTimer > 0.3f)
                {
                    AudioManager.instance.PlayOneShot(FMODEvents.instance.land, player.transform.position);
                    player.ChangeMovementHandlerToGrounded();
                }            
            }
            else
            {
                groundedTimer = 0f;
            }

            // Double jump
            if (jumpTimer > 0.0f) jumpTimer -= 0.1f;

            if (canDoubleJump && doubleJumps < player.GetJumps() - 1 && player.HasPressedJump() && jumpTimer <= 0f)
            {
                Vector3 velocity = player.GetRigidbody().velocity;
                velocity.y = jumpStrength;
                Vector3 forwardVelocity = orientation.transform.forward * (jumpForwardStrength * player.GetYInput());
                velocity += forwardVelocity;
                player.GetRigidbody().velocity = velocity;
                jumpTimer = 0.8f;
                doubleJumps++;
                canDoubleJump = false;
                player.GetCamera().FovWarp(2.8f, .35f);
                AudioManager.instance.PlayOneShot(FMODEvents.instance.dash, player.transform.position);
            }

            if (!canDoubleJump && !player.HasPressedJump() && jumpTimer <= 0f)
            {
                canDoubleJump = true;
            }

            // Slide down steep slopes
            RaycastHit groundHit = player.GetGroundHit();
            
            if (groundHit.distance < slideDistance && Vector3.Angle(groundHit.normal, Vector3.up) > slideAngle)
            {
                player.GetRigidbody().AddForce(Vector3.down * slideSpeed, ForceMode.Acceleration);
            }

            //Cap velocity
            if (player.GetRigidbody().velocity.magnitude > maxVelocity)
            {
                player.GetRigidbody()
                    .AddForce(player.GetRigidbody().velocity.normalized *
                              (maxVelocity - player.GetRigidbody().velocity.magnitude), ForceMode.VelocityChange);
            }
        }

        public bool ShouldHoverApply(PlayerCharacterController player)
        {
            return false;
        }
    }
}