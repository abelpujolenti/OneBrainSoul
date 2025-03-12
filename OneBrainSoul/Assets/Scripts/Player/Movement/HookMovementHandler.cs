using ECS.Entities.AI;
using Managers;
using Player.Effects;
using UnityEngine;

namespace Player.Movement
{
    public class HookMovementHandler : IMovementHandler
    {
        private Hitstop _hitstop;

        public static float speed = 120f;
        public static float jumpStrength = 14f;
        public static float endDistance = 1.5f;
        public static float speedFalloff = 77.5f;
        public static float speedFalloffPower = 3f;
        public static float bobbingStrength = .65f;
        public static float snapDownwardsStrength = 6f;
        public static float smashAdditionalSpeed = 90f;
        public static uint hookDamage = 1;
        public static uint smashDamage = 2;

        public static float bounceStrength = 1900f;
        public static float bounceVerticalRatio = .45f;

        public static float delay = 0.2f;
        public static float delayHitImpact = 0.025f;


        public static int lineVertices = 2;

        Vector3 startPos;
        Vector3 endPos;
        Vector3 endVisualPos;
        Vector3 lineStartPos;
        bool snap;
        float hookDistance;
        float delayTime = 0f;
        float bobbingCycle = 0f;
        bool smash = false;
        Vector3 movementDirection = Vector3.zero;
        LineRenderer line;
        Transform hookParticleTransform;
        ParticleSystem smashParticle;

        public HookMovementHandler(LineRenderer lineRenderer, ParticleSystem smashParticle, Transform hookParticle, Hitstop hitstop)
        {
            line = lineRenderer;
            hookParticleTransform = hookParticle;
            this.smashParticle = smashParticle;
            _hitstop = hitstop;
        }

        public void Setup(PlayerCharacterController player, Vector3 startPos, Vector3 endPos, Vector3 endVisualPos, bool snap, bool smash)
        {
            this.startPos = startPos;
            this.endPos = endPos;
            this.endVisualPos = endVisualPos;
            this.snap = snap;
            this.smash = smash;
            hookDistance = Vector3.Distance(this.endPos, this.startPos);
            line.enabled = true;
            line.positionCount = lineVertices;
            lineStartPos = GetLineStartPos(player);
            hookParticleTransform.position = lineStartPos;
            hookParticleTransform.gameObject.SetActive(true);
            hookParticleTransform.GetComponent<ParticleSystem>().Play();
        }

        private Vector3 GetLineStartPos(PlayerCharacterController player)
        {
            return player.transform.position + new Vector3(0f, 1f, 0f) + player.GetOrientation().right * -1.5f;
        }

        public void ResetValues()
        {
            delayTime = 0;
            bobbingCycle = 0;
            movementDirection = Vector3.zero;
        }

        public void VisualUpdate(PlayerCharacterController player)
        {
            lineStartPos = GetLineStartPos(player);
            //line.SetPosition(0, lineStartPos);

            Vector3 lineEndPos = lineStartPos + (endVisualPos - lineStartPos) * Mathf.Pow(Mathf.Clamp01(delayTime / delay), 2f);
            hookParticleTransform.transform.position = lineEndPos;

            if (delayTime < delay)
            {
                //Vector3 lineEndPos = lineStartPos + (endVisualPos - lineStartPos) * Mathf.Pow(delayTime / delay, 2f);
                //particle.transform.position = lineEndPos;
                //line.SetPosition(1, lineEndPos);
            }
        }

        public void Move(PlayerCharacterController player)
        {
            float distanceToTarget = Vector3.Distance(endPos, player.transform.position);
            //Throw hook
            if (delayTime < delay + delayHitImpact)
            {
                if (delayTime < delay)
                {
                    if (delayTime + Time.fixedDeltaTime >= delay)
                    {
                        //sound
                    }
                }
                else
                {
                    if (delayTime + Time.fixedDeltaTime >= delay + delayHitImpact)
                    {
                        startPos = player.transform.position;
                        movementDirection = (endPos - startPos).normalized;
                        hookDistance = Vector3.Distance(endPos, startPos);
                        player.GetCamera().FovWarp(2f * 60f / distanceToTarget, 2.5f);
                        player.GetRigidbody().AddForce(Mathf.Max(0f, -player.GetRigidbody().velocity.y) * Vector3.up, ForceMode.VelocityChange);

                        if (smash)
                        {
                            PostProcessingManager.Instance.ChargeRunEffect(distanceToTarget * 7f / (speed + smashAdditionalSpeed));
                        }
                    }
                }

                delayTime += Time.fixedDeltaTime;
                return;
            }

            float progress = 1f - Mathf.Min(1f, distanceToTarget / hookDistance);
            float speedWithFalloff = speed - Mathf.Pow(progress, 1f / speedFalloffPower) * speedFalloff;
            speedWithFalloff = smash ? speedWithFalloff + smashAdditionalSpeed : speedWithFalloff;
            player.GetRigidbody().velocity = movementDirection * speedWithFalloff;

            bobbingCycle += Time.fixedDeltaTime * player.GetRigidbody().velocity.magnitude / 2f;

            player.GetRigidbody().AddTorque(
                Quaternion.Euler(0, 90, 0) * new Vector3(movementDirection.x, 0f, movementDirection.z).normalized *
                (player.GetRigidbody().velocity.magnitude * bobbingStrength), ForceMode.Acceleration);
            
            //float sidewaysBobbingMagnitude = Mathf.Sin(bobbingCycle) * player.GetRigidbody().velocity.magnitude * bobbingStrength / 8f;
            //player.rb.AddTorque(movementDirection * sidewaysBobbingMagnitude, ForceMode.Acceleration);

            //End
            if (distanceToTarget < endDistance || distanceToTarget > hookDistance * 2f)
            {
                if (snap)
                {
                    Vector3 dir = (endPos - startPos).normalized;
                    dir.y *= -1f;
                    Vector3 snapDir = (Vector3.down + dir.normalized).normalized;
                    Vector3 snapForce = snapDir * (snapDownwardsStrength * player.GetRigidbody().velocity.magnitude *
                                                   Mathf.Max(0f, Vector3.Dot(player.GetRigidbody().velocity, Vector3.up)));
                    player.GetRigidbody().AddForce(snapForce);
                }

                Exit(player);
            }

            RaycastHit hit;
            CapsuleCollider playerCollider = player.GetCapsuleCollider();
            Vector3 p1 = player.transform.position + playerCollider.center + Vector3.up * (-playerCollider.height * 0.5f);
            Vector3 p2 = p1 + Vector3.up * playerCollider.height;
            if (Physics.CapsuleCast(p1, p2, playerCollider.radius, player.GetRigidbody().velocity.normalized, out hit, 
                    player.GetRigidbody().velocity.magnitude * 0.04f, GameManager.Instance.GetRaycastLayersWithoutAlly(), 
                    QueryTriggerInteraction.Ignore))
            {
                if (smash)
                {
                    Smash(player, hit);
                }
                else
                {
                    AgentEntity entity = hit.collider.GetComponent<AgentEntity>();
                    if (entity != null)
                    {
                        entity.OnReceiveDamage(hookDamage, hit.point, player.transform.position);
                    }
                }

                Exit(player);
            }

            // Jump and switch state to airborne
            if (distanceToTarget < hookDistance * .6 && player.HasPressedJump())
            {
                Vector3 velocity = player.GetRigidbody().velocity;
                velocity.y = jumpStrength;
                player.GetRigidbody().velocity = velocity;
                Exit(player);

                return;
            }
        }

        private void Exit(PlayerCharacterController player)
        {
            line.enabled = false;
            hookParticleTransform.gameObject.SetActive(false);
            hookParticleTransform.position = GetLineStartPos(player);

            if (!player.IsOnTheGround())
            {
                player.ChangeMovementHandlerToAirborne();
            }
            else
            {
                player.ChangeMovementHandlerToGrounded();
            }
        }

        private void Smash(PlayerCharacterController player, RaycastHit hit)
        {
            player.GetCamera().ScreenShake(.2f, .8f);

            DestructibleTerrain destructibleTerrain = hit.collider.GetComponent<DestructibleTerrain>();
            bool hitTerrain = destructibleTerrain != null;
            if (hitTerrain)
            {
                destructibleTerrain.Break(hit.point);
            }
            AgentEntity entity = hit.collider.GetComponent<AgentEntity>();
            if (entity != null)
            {
                entity.OnReceiveDamage(smashDamage, hit.point, player.transform.position);
            }

            bool damaged = hitTerrain || entity != null;

            var em = smashParticle.emission;
            var b = em.GetBurst(0);
            b.count = damaged ? 20 : 8;
            em.SetBurst(0, b);
            smashParticle.Play();

            PostProcessingManager.Instance.ChargeCollideEffect((damaged ? .12f : .065f) + .3f);
            AudioManager.instance.PlayOneShot(FMODEvents.instance.hammerAttack, player.transform.position);
            AudioManager.instance.PlayOneShot(FMODEvents.instance.enemyDamage, player.transform.position);

            player.GetCamera().StopFovWarp();
            _hitstop.Add(damaged ? .2f : .12f);
            player.GetCamera().ScreenShake(damaged ? .25f : .2f, damaged ? 1.3f : .8f);
            _hitstop.AddAftershock(damaged ? .23f : .2f);
            player.GetRigidbody().velocity = Vector3.zero;
            player.GetRigidbody().AddForce(
                (new Vector3(hit.normal.x, Mathf.Max(0f, hit.normal.y), hit.normal.z).normalized +
                 new Vector3(0f, bounceVerticalRatio * (damaged ? 1.2f : 1f), 0f)).normalized *
                (bounceStrength * (damaged ? 1.4f : 1f)), ForceMode.Acceleration);
        }

        public bool ShouldGravityApply(PlayerCharacterController player)
        {
            return delayTime < delay;
        }

        public bool ShouldHoverApply(PlayerCharacterController player)
        {
            return delayTime < delay;
        }
    }
}
