using System;
using System.Collections.Generic;
using ECS.Entities.AI;
using Managers;
using Player;
using Player.Effects;
using UnityEngine;

namespace Combat
{
    public class Weapon : MonoBehaviour
    {
        public float moveSpeed = 5f;
        public float rotSpeed = 5f;
        public float duration = .8f;
        public float strength = 1.8f;
        public float cancelAnimationTime = .3f;

        public uint baseDamage = 1;
        public float activeStart = 0.3f;
        public float activeEnd = 0.6f;
        public float range = 3f;
        public float outerArc = 45f;
        public float innerRange = .5f;
        public float innerArc = 150f;
        public float hitstop = .05f;

        public float soundAreaRadius;

        [SerializeField] protected Transform parent;
        [SerializeField] protected PlayerCharacterController player;
        [SerializeField] protected Hitstop _hitstop;
        [SerializeField] private AnimationCurve animationCurve;
        public float animationTimer { get; private set; }
        public float animationSpeed { get; private set; }
        private Vector3 targetPos;
        private Quaternion targetRot;
        protected bool attackLanded = false;

        private void OnGUI()
        {
            Event e = Event.current;
            if ((e.type == EventType.MouseDown && Input.GetMouseButtonDown(0)))
            {
                if (animationTimer < cancelAnimationTime)
                {
                    AttackCommand();
                    CombatManager.Instance.PlayerAttackSoundArea(transform.position, soundAreaRadius);
                }
            }
        }
        
        void Update()
        {
            //TODO ADRI ATTACK_UPDATE HAURIA D'ANAR A ONGUI()
            AttackUpdate();
            AnimationUpdate();
        }

        private void AnimationUpdate()
        {
            float moveSpeedAdjusted = moveSpeed;
            float rotSpeedAdjusted = rotSpeed;
            if (animationTimer > 0f)
            {
                animationTimer = Math.Clamp(animationTimer - Time.deltaTime * animationSpeed, 0f, 1f);

                float animationValue = animationCurve.Evaluate(animationTimer);
                targetPos = new Vector3(animationValue * -0.2f, animationValue * -0.2f, animationValue * 0.7f);//new Vector3(0f, animationValue * 0.3f, animationValue * 0.6f);
                targetRot = Quaternion.AngleAxis(30f * animationValue, new Vector3(1f, -.4f * animationValue * animationValue, 0f).normalized);
            }
            else
            {
                targetPos = Vector3.zero;
                targetRot = Quaternion.identity;
            }

            Vector3 posDiff = targetPos - parent.localPosition;
            float allowedPos = moveSpeedAdjusted * posDiff.magnitude * Time.deltaTime;
            parent.localPosition += Vector3.ClampMagnitude(posDiff, allowedPos);
            parent.localRotation = Quaternion.RotateTowards(parent.localRotation, targetRot, rotSpeedAdjusted * Time.deltaTime * Quaternion.Angle(parent.localRotation, targetRot));
        }

        protected virtual void AttackUpdate()
        {

        }

        protected virtual void AttackLand(List<AgentEntity> affectedEntities)
        {
            foreach (AgentEntity affectedEntity in affectedEntities)
            {
                AttackLand(affectedEntity);
            }
        
            _hitstop.Add(hitstop * (.8f + affectedEntities.Count * .2f));
            attackLanded = true;
        }

        protected virtual void AttackLand(AgentEntity effectedEntity)
        {
            effectedEntity.OnReceiveDamage(baseDamage, 
                player.GetCamera().transform.position +
                (effectedEntity.transform.position - player.GetCamera().transform.position).normalized *
                ((effectedEntity.transform.position - player.GetCamera().transform.position).magnitude - effectedEntity.GetRadius()), 
                transform.position);
        }

        protected virtual void AttackCommand()
        {
            SetAnimation(AnimationCurve.EaseInOut(0f, 0f, 1f, strength), duration);
            attackLanded = false;
        }
    
    
        protected void SetAnimation(AnimationCurve curve, float animationLength)
        {
            animationCurve = curve;
            animationSpeed = 1f / animationLength;
            animationTimer = 1f;
        }

        public void Activate()
        {
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }
    }
}
