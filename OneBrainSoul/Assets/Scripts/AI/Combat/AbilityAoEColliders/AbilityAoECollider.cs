using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using AI.Combat.AbilitySpecs;
using ECS.Components.AI.Combat.Abilities;
using ECS.Entities.AI;
using Interfaces.AI.Combat;
using Managers;
using UnityEngine;

namespace AI.Combat.AbilityAoEColliders
{
    public abstract class AbilityAoECollider<TAbilityComponent> : MonoBehaviour
        where TAbilityComponent : AbilityComponent
    {
        private Coroutine _castingCoroutine;
        
        protected Quaternion _parentRotation;
        
        protected Stopwatch _stopwatch = new Stopwatch();
        
        protected List<AgentEntity> _agentsInside = new List<AgentEntity>();
        
        private Action<AgentEntity> _actionOnTriggerEffect;

        protected Vector3 _relativePosition;

        protected Action _actionAttaching;
        
        private uint _value;

        private float _duration;
        private float _tickTimer;
        
        protected abstract void OnEnable();
        protected abstract void OnDisable();
        
        public virtual void SetAbilitySpecs(TAbilityComponent abilityComponent)
        {
            _relativePosition = abilityComponent.GetCast().relativeSpawnPosition;

            _actionAttaching = abilityComponent.GetCast().isAttachedToCaster
                ? () => { }
                : () =>
                {
                    transform.parent = null;
                };
            
            _value = abilityComponent.GetEffect().value;
            _duration = abilityComponent.GetEffect().duration;
            
            Setup(abilityComponent.GetEffectType());
        }

        private void Setup(AbilityEffectType abilityEffectType)
        {
            switch (abilityEffectType)
            {
                case AbilityEffectType.DIRECT_DAMAGE:
                    
                    _actionOnTriggerEffect = agentEntity => Damage(agentEntity, _value, Vector3.zero);
                    
                    break;
                    
                case AbilityEffectType.DAMAGE_PER_TICKS:

                    _actionOnTriggerEffect = agentEntity => Damage(agentEntity, _value, Vector3.zero);
                    _tickTimer = GameManager.Instance.GetTimeBetweenDamageTicks(); 

                    break;
                
                case AbilityEffectType.DIRECT_HEAL:
                    
                    _actionOnTriggerEffect = agentEntity => Heal(agentEntity, _value);
                    
                    break;
                    
                case AbilityEffectType.HEAL_PER_TICKS:
                    
                    _actionOnTriggerEffect = agentEntity => Heal(agentEntity, _value);
                    _tickTimer = GameManager.Instance.GetTimeBetweenHealTicks();
                        
                    break;
                
                case AbilityEffectType.SLOW:

                    _actionOnTriggerEffect = agentEntity => Slow(agentEntity, _value);
                    
                    break;
            }
        }

        public abstract void SetAbilityTargets(int targetsLayerMask);

        private void Damage(IDamageable agent, uint damageValue, Vector3 hitPosition)
        {
            agent.OnReceiveDamage(damageValue, hitPosition);
        }

        private void Push(IPushable agent, Vector3 forceDirection, float forceStrength)
        {
            agent.OnReceivePush(forceDirection, forceStrength);
        }

        private void Heal(IHealable agent, uint healValue)
        {
            agent.OnReceiveHeal(healValue);
        }

        private void Slow(ISlowable agent, uint slowPercent)
        {
            agent.OnReceiveSlow(slowPercent);
        }

        public void SetParent(Transform parentTransform)
        {
            transform.parent = parentTransform;
            _parentRotation = parentTransform.rotation;
        }

        protected void MoveToPosition(Vector3 position)
        {
            gameObject.transform.localPosition = position;
        }

        public void Activate()
        {
            gameObject.SetActive(true);
            _castingCoroutine = StartCoroutine(DurationTimeCoroutine(_duration));
        }

        private IEnumerator DurationTimeCoroutine(float duration)
        {
            float timer = 0;
            float tickTimer = 0;

            yield return new WaitForFixedUpdate();
            
            TriggerEffect();
            
            timer += Time.deltaTime;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                tickTimer += Time.deltaTime;

                if (tickTimer >= _tickTimer)
                {
                    TriggerEffect();
                    tickTimer = 0;
                }
                yield return null;
            }
            
            Deactivate();
        }

        private void TriggerEffect()
        {
            foreach (AgentEntity agentEntity in _agentsInside)
            {
                _actionOnTriggerEffect(agentEntity);
            }
        }

        private void Deactivate()
        {
            _agentsInside.Clear();
            _stopwatch.Reset();
            gameObject.SetActive(false);
        }
    }
}