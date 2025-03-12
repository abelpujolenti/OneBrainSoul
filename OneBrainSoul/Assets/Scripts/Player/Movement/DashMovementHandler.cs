using ECS.Entities.AI;
using Managers;
using UnityEngine;

namespace Player.Movement
{
    public class DashMovementHandler : IMovementHandler
    {
        public static float dashSpeed = 2700f;
        public static float dashSpeedFalloff = 800f;
        public static float dashSpeedFalloffPower = 2f;
        public static float horizontalDrag = 35f;
        public static float horizontalAirDrag = 25f;
        public static float bobbingStrength = 0.6f;
        public static float duration = .09f;
        public static uint damage = 1;

        private float chargeTime = 0f;
        private float bobbingCycle = 0f;
        Vector3 dashDirection;

        public void Setup(PlayerCharacterController playerCharacterController, Vector3 dashDirection)
        {
            this.dashDirection = dashDirection;
            playerCharacterController.GetCamera().FovWarp(.8f / duration, 1.22f);
        }

        public void ResetValues()
        {
            chargeTime = 0;
            bobbingCycle = 0;
        }

        public void Move(PlayerCharacterController player)
        {
            Transform orientation = player.GetOrientation();
            Rigidbody rigidbody = player.GetRigidbody();
            
            bobbingCycle += Time.fixedDeltaTime * rigidbody.velocity.magnitude / 2f;
            rigidbody.AddTorque(Quaternion.Euler(0, 90, 0) * rigidbody.velocity * bobbingStrength, ForceMode.Acceleration);

            Quaternion groundNormalRotation = Quaternion.FromToRotation(Vector3.up, player.GetGroundHit().normal);
            Vector3 dashForwardDirection = (player.IsOnTheGround() ? groundNormalRotation : Quaternion.identity) * dashDirection;
            float chargeSpeedWithFalloff = dashSpeed - Mathf.Pow(chargeTime / duration, 1f / dashSpeedFalloffPower) * dashSpeedFalloff;
            rigidbody.AddForce(dashForwardDirection * chargeSpeedWithFalloff, ForceMode.Acceleration);

            chargeTime += Time.fixedDeltaTime;
            if (chargeTime >= duration)
            {
                Exit(player);
                return;
            }

            // Drag, opposite to current horizontal velocity
            Vector3 horizontalVelocity = rigidbody.velocity;
            horizontalVelocity.y = 0;
            rigidbody.AddForce(-horizontalVelocity * (player.IsOnTheGround() ? horizontalDrag : horizontalAirDrag), ForceMode.Acceleration);

            RaycastHit hit;
            if (Physics.Raycast(player.transform.position, dashForwardDirection, out hit, rigidbody.velocity.magnitude * 0.04f, GameManager.Instance.GetRaycastLayers(), QueryTriggerInteraction.Ignore))
            {
                DamageTakingEntity entity = hit.collider.GetComponent<DamageTakingEntity>();
                if (entity != null)
                {
                    entity.Damage(player, hit.point, 1);
                }

                bool damaged = entity != null;
                Exit(player);
                return;
            }

            CapsuleCollider playerCollider = player.GetCapsuleCollider();
            Vector3 p1 = player.transform.position + playerCollider.center + Vector3.up * (-playerCollider.height * 0.5f);
            Vector3 p2 = p1 + Vector3.up * playerCollider.height;
            if (Physics.CapsuleCast(p1, p2, playerCollider.radius, player.GetRigidbody().velocity.normalized, out hit,
                    player.GetRigidbody().velocity.magnitude * 0.1f, GameManager.Instance.GetRaycastLayersWithoutAlly(),
                    QueryTriggerInteraction.Ignore))
            {
                AgentEntity entity = hit.collider.GetComponent<AgentEntity>();
                if (entity != null)
                {
                    entity.OnReceiveDamage(damage, hit.point, player.transform.position);
                }
                Exit(player);
            }
        }

        private void Exit(PlayerCharacterController player)
        {
            if (!player.IsOnTheGround())
            {
                player.ChangeMovementHandlerToAirborne();
            }
            else
            {
                player.ChangeMovementHandlerToGrounded();
            }
        }
    }
}
