using ECS.Entities.AI;
using Managers;
using Player.Effects;
using UnityEngine;

namespace Player.Movement
{
    public class ChargeMovementHandler : IMovementHandler
    {
        private Hitstop _hitstop;
        
        public static float chargeSpeed = 1000f;
        public static float chargeSpeedFalloff = 575f;
        public static float chargeSpeedFalloffPower = 4f;
        public static float swerveSpeed = 280f;
        public static float horizontalDrag = 15f;
        public static float bobbingStrength = 0.6f;
        public static float duration = .7f;
        public static float bounceStrength = 1900f;
        public static float bounceVerticalRatio = .45f;

        private float chargeTime = 0f;
        private float bobbingCycle = 0f;
        Vector3 chargeDirection;

        public ChargeMovementHandler(Hitstop hitstop)
        {
            _hitstop = hitstop;
        }

        public void Setup(PlayerCharacterController playerCharacterController, Vector3 chargeDirection)
        {
            playerCharacterController.GetCamera().FovWarp(1f, 2f);
            this.chargeDirection = chargeDirection;
            PostProcessingManager.Instance.ChargeRunEffect(duration);
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

            Vector3 swerveDirection = (orientation.right * player.GetXInput()).normalized;
            swerveDirection = Quaternion.FromToRotation(Vector3.up, player.GetGroundHit().normal) * swerveDirection;
            rigidbody.AddForce(swerveDirection * swerveSpeed, ForceMode.Acceleration);

            bobbingCycle += Time.fixedDeltaTime * rigidbody.velocity.magnitude / 2f;
            rigidbody.AddTorque(Quaternion.Euler(0, 90, 0) * rigidbody.velocity * bobbingStrength, ForceMode.Acceleration);
            float sidewaysBobbingMagnitude = Mathf.Sin(bobbingCycle) * rigidbody.velocity.magnitude * bobbingStrength / 8f;
            rigidbody.AddTorque(chargeDirection * sidewaysBobbingMagnitude, ForceMode.Acceleration);

            Vector3 chargeForwardDirection = Quaternion.FromToRotation(Vector3.up, player.GetGroundHit().normal) * chargeDirection;
            float chargeSpeedWithFalloff = chargeSpeed - Mathf.Pow(chargeTime / duration, 1f / chargeSpeedFalloffPower) * chargeSpeedFalloff;
            rigidbody.AddForce(chargeForwardDirection * chargeSpeedWithFalloff, ForceMode.Acceleration);

            chargeTime += Time.fixedDeltaTime;
            if (!player.IsOnTheGround())
            {
                Exit(player);
                return;
            }
            else if (chargeTime >= duration)
            {
                Exit(player);
                return;
            }

            // Drag, opposite to current horizontal velocity
            Vector3 horizontalVelocity = rigidbody.velocity;
            horizontalVelocity.y = 0;
            rigidbody.AddForce(-horizontalVelocity * horizontalDrag, ForceMode.Acceleration);

            RaycastHit hit;
            if (Physics.Raycast(player.transform.position + 0.5f * Vector3.up, chargeDirection, out hit, 
                    rigidbody.velocity.magnitude * 0.04f, GameManager.Instance.GetRaycastLayersWithoutAlly(), 
                    QueryTriggerInteraction.Ignore) && 
                Vector3.Angle(hit.normal, Vector3.up) > AirborneMovementHandler.slideAngle)
            {
                DestructibleTerrain destructibleTerrain = hit.collider.GetComponent<DestructibleTerrain>();
                bool hitTerrain = destructibleTerrain != null;
                if (hitTerrain)
                {
                    destructibleTerrain.Break(hit.point);
                }

                AgentEntity agentEntity = hit.collider.GetComponent<AgentEntity>();
                if (agentEntity)
                {
                    //TODO CHANGE CHARGE DAMAGE
                    agentEntity.OnReceiveDamage(20, hit.point, player.transform.position);
                }

                bool damaged = hitTerrain || agentEntity;
                Collide(player, rigidbody, hit.normal, damaged);
                //Exit(player);
                return;
            }
        }

        private void Collide(PlayerCharacterController player, Rigidbody rigidbody, Vector3 normal, bool damaged)
        {
            PostProcessingManager.Instance.ChargeCollideEffect((damaged ? .12f : .065f) + .3f);
            AudioManager.instance.PlayOneShot(FMODEvents.instance.hammerAttack, player.transform.position);
            AudioManager.instance.PlayOneShot(FMODEvents.instance.enemyDamage, player.transform.position);

            player.GetCamera().StopFovWarp();
            _hitstop.Add(damaged ? .2f : .12f);
            player.GetCamera().ScreenShake(damaged ? .25f : .2f, damaged ? 1.3f : .8f);
            _hitstop.AddAftershock(damaged ? .23f : .2f);
            rigidbody.velocity = Vector3.zero;
            rigidbody.AddForce(
                (new Vector3(normal.x, Mathf.Max(0f, normal.y), normal.z).normalized +
                 new Vector3(0f, bounceVerticalRatio * (damaged ? 1.2f : 1f), 0f)).normalized *
                (bounceStrength * (damaged ? 1.4f : 1f)), ForceMode.Acceleration);


            player.ChangeMovementHandlerToAirborne();
            player.SetHorizontalDrag(5f);
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
