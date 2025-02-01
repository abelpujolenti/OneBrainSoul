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
using Debug = UnityEngine.Debug;

namespace AI.Combat.AbilityAoEColliders
{
    public abstract class AbilityAoECollider<TAbilityComponent> : MonoBehaviour
        where TAbilityComponent : AbilityComponent
    {
        private Coroutine _abilityDurationCoroutine;

        protected Transform _parentTransform;
        
        private Stopwatch _stopwatch = new Stopwatch();
        
        private List<AgentEntity> _agentsInside = new List<AgentEntity>();

        private List<Action<AgentEntity>> _actionsOnTriggerEnter = new List<Action<AgentEntity>>();
        private List<Action<AgentEntity>> _actionsOnTriggerStay = new List<Action<AgentEntity>>();
        private List<Action<AgentEntity>> _actionsOnTriggerExit = new List<Action<AgentEntity>>();

        private Vector3 _relativePosition;

        private Action _actionAttaching;
        private Action _actionOnStartTrigger = () => { };
        private Action<AgentEntity> _actionOnTriggerEnter = agentEntity => { };
        protected Action<float> _actionResizing = value => { };
        private Action<AgentEntity> _actionOnTriggerExit  = agentEntity => { };

        private float _abilityDuration;
        private float _tickTimer;

        protected virtual void OnEnable()
        {
            _stopwatch.Start();
            MoveToPosition(_relativePosition);
            _actionAttaching();
        }

        protected virtual void OnDisable()
        {
            _stopwatch.Reset();
        }

        public virtual void SetAbilitySpecs(Transform parentTransform, TAbilityComponent abilityComponent)
        {
            _parentTransform = parentTransform;
            
            _abilityDuration = abilityComponent.GetAoE().duration;
            
            _relativePosition = abilityComponent.GetCast().relativeSpawnPosition;

            _actionAttaching = abilityComponent.GetCast().isAttachedToCaster
                ? () => { }
                : () =>
                {
                    SetParent(_parentTransform);
                    transform.parent = null;
                };

            AbilityTrigger abilityTrigger = abilityComponent.GetTrigger();

            if (abilityTrigger.doesEffectOnStart)
            {
                SetupOnEnterExit(_actionsOnTriggerEnter, abilityComponent.GetEffectTypeOnStart(), abilityComponent.GetEffectOnStart());

                if (abilityComponent.DoesItTriggerOnTriggerEnter())
                {
                    _actionOnTriggerEnter = TriggerOnEnterEffect;
                }
                else
                {
                    _actionOnStartTrigger = TriggerOnEnterEffect;
                }
            }

            if (abilityTrigger.doesEffectOnTheDuration)
            {
                SetupOnStay(abilityComponent.GetEffectTypeOnTheDuration(), abilityComponent.GetEffectOnTheDuration());
            }

            if (!abilityTrigger.doesEffectOnEnd)
            {
                return;
            }
            
            SetupOnEnterExit(_actionsOnTriggerExit, abilityComponent.GetEffectTypeOnEnd(), abilityComponent.GetEffectOnEnd());
            
            if (!abilityComponent.DoesItTriggerOnTriggerExit())
            {
                return;
            }
            _actionOnTriggerExit = TriggerOnExitEffect;
        }

        private void SetupOnEnterExit(List<Action<AgentEntity>> actionsList, AbilityEffectOnHealthType abilityEffectOnHealthType, 
            AbilityEffect abilityEffect)
        {
            if (abilityEffect.hasAnEffectOnHealth)
            {
                switch (abilityEffectOnHealthType)
                {
                    case AbilityEffectOnHealthType.DAMAGE:
                        if (abilityEffect.isEffectOnHealthAttachedToEntity)
                        {
                            AddDamageOverTime(actionsList, abilityEffect.healthModificationValue, abilityEffect.effectOnHealthDuration);
                            break;
                        }
                        AddDamage(actionsList, abilityEffect.healthModificationValue);
                        break;
                
                    case AbilityEffectOnHealthType.HEAL:
                        if (abilityEffect.isEffectOnHealthAttachedToEntity)
                        {
                            AddHealOverTime(actionsList, abilityEffect.healthModificationValue, abilityEffect.effectOnHealthDuration);
                            break;
                        }
                    
                        AddHeal(actionsList, abilityEffect.healthModificationValue);
                        break;
                }
            }
            
            SetupSlowAndForce(actionsList, abilityEffect);
        }

        private void SetupOnStay(AbilityEffectOnHealthType abilityEffectOnHealthType, AbilityEffect abilityEffect)
        {
            if (abilityEffect.hasAnEffectOnHealth)
            {
                switch (abilityEffectOnHealthType)
                {
                    case AbilityEffectOnHealthType.DAMAGE:
                        AddDamage(_actionsOnTriggerStay, abilityEffect.healthModificationValue);
                        _tickTimer = GameManager.Instance.GetTimeBetweenDamageTicks();
                        break;
                
                    
                    case AbilityEffectOnHealthType.HEAL:
                        AddHeal(_actionsOnTriggerStay, abilityEffect.healthModificationValue);
                        _tickTimer = GameManager.Instance.GetTimeBetweenHealTicks();
                        break;
                }
            }
            
            SetupSlowAndForce(_actionsOnTriggerStay, abilityEffect);
        }

        private void SetupSlowAndForce(List<Action<AgentEntity>> actionsList, AbilityEffect abilityEffect)
        {
            if (abilityEffect.doesSlow)
            {
                if (!abilityEffect.isSlowAttachedToEntity)
                {
                    AddSlow(actionsList, abilityEffect.slowPercent);
                    _actionsOnTriggerExit.Add(ReleaseFromSlow);
                }
                else
                {
                    AddSlowOverTime(actionsList, abilityEffect.slowPercent, abilityEffect.slowDuration, 
                        abilityEffect.doesDecreaseOverTime);
                }
            }

            if (!abilityEffect.doesApplyAForce)
            {
                return;
            }
            AddPush(actionsList, abilityEffect.forceDirection, abilityEffect.forceStrength);
        }

        public abstract void SetAbilityTargets(int targetsLayerMask);

        private void AddDamage(List<Action<AgentEntity>> actionsList, uint damageValue)
        {
            //TODO HIT POSITION
            actionsList.Add(agentEntity => Damage(agentEntity, damageValue, Vector3.zero));
        }

        private void Damage(IDamageable agent, uint damageValue, Vector3 hitPosition)
        {
            agent.OnReceiveDamage(damageValue, hitPosition);
        }

        private void AddDamageOverTime(List<Action<AgentEntity>> actionsList, uint damageValue, float duration)
        {
            actionsList.Add(agentEntity => { DamageOverTime(agentEntity, damageValue, duration); });
        }

        private void DamageOverTime(IDamageable agent, uint damageValue, float duration)
        {
            agent.OnReceiveDamageOverTime(damageValue, duration);
        }

        private void AddHeal(List<Action<AgentEntity>> actionsList, uint healValue)
        {
            actionsList.Add(agentEntity => { Heal(agentEntity, healValue); });
        }

        private void Heal(IHealable agent, uint healValue)
        {
            agent.OnReceiveHeal(healValue);
        }

        private void AddHealOverTime(List<Action<AgentEntity>> actionsList, uint healValue, float duration)
        {
            actionsList.Add(agentEntity => { HealOverTime(agentEntity, healValue, duration); });
        }

        private void HealOverTime(IHealable agent, uint healValue, float duration)
        {
            agent.OnReceiveHealOverTime(healValue, duration);
        }

        private void AddSlow(List<Action<AgentEntity>> actionsList, uint slowPercent)
        {
            actionsList.Add(agentEntity => { Slow(agentEntity, slowPercent); });
        }

        private void Slow(ISlowable agent, uint slowPercent)
        {
            agent.OnReceiveSlow(slowPercent);
        }

        private void AddSlowOverTime(List<Action<AgentEntity>> actionsList, uint slowPercent, float duration, bool doesDecrease)
        {
            if (doesDecrease)
            {
                actionsList.Add(agentEntity => { SlowOverTime(agentEntity, slowPercent, duration); });
                return;
            }
            
            actionsList.Add(agentEntity => { DecreasingSlow(agentEntity, slowPercent, duration); });
        }

        private void SlowOverTime(ISlowable agent, uint slowPercent, float duration)
        {
            agent.OnReceiveSlowOverTime(slowPercent, duration);
        }

        private void DecreasingSlow(ISlowable agent, uint slowPercent, float duration)
        {
            agent.OnReceiveDecreasingSlow(slowPercent, duration);
        }

        private void AddPush(List<Action<AgentEntity>> actionsList, Vector3 forceDirection, float forceStrength)
        {
            actionsList.Add(agentEntity => { Push(agentEntity, forceDirection, forceStrength); });
        }

        private void Push(IPushable agent, Vector3 forceDirection, float forceStrength)
        {
            agent.OnReceivePush(forceDirection, forceStrength);
        }

        private void AddReleaseFromSlow(List<Action<AgentEntity>> actionsList)
        {
            actionsList.Add(ReleaseFromSlow);
        }

        private void ReleaseFromSlow(ISlowable agent)
        {
            agent.OnReleaseFromSlow();
        }

        public void SetParent(Transform parentTransform)
        {
            transform.parent = parentTransform;
        }

        private void MoveToPosition(Vector3 position)
        {
            gameObject.transform.localPosition = position;
        }

        public void Activate()
        {
            _actionResizing(0);
            gameObject.SetActive(true);
            _abilityDurationCoroutine = StartCoroutine(AbilityDurationCoroutine());
        }

        private IEnumerator AbilityDurationCoroutine()
        {
            float timer = 0;
            float tickTimer = 0;

            yield return new WaitForFixedUpdate();
            
            _actionOnStartTrigger();
            
            timer += Time.deltaTime;

            while (timer < _abilityDuration)
            {
                timer += Time.deltaTime;
                tickTimer += Time.deltaTime;

                _actionResizing(timer);

                if (tickTimer >= _tickTimer)
                {
                    TriggerOnStayEffect();
                    tickTimer = 0;
                }
                yield return null;
            }
            
            TriggerOnExitEffect();
            
            Deactivate();
        }

        private void TriggerOnEnterEffect()
        {
            foreach (AgentEntity agentEntity in _agentsInside)
            {
                TriggerOnEnterEffect(agentEntity);
            }
        }

        private void TriggerOnEnterEffect(AgentEntity agentEntity)
        {
            foreach (Action<AgentEntity> action in _actionsOnTriggerEnter)
            {
                action(agentEntity);
            }
        }

        private void TriggerOnStayEffect()
        {
            foreach (AgentEntity agentEntity in _agentsInside)
            {
                foreach (Action<AgentEntity> action in _actionsOnTriggerStay)
                {
                    action(agentEntity);
                }
            }
        }

        private void TriggerOnExitEffect()
        {
            foreach (AgentEntity agentEntity in _agentsInside)
            {
                TriggerOnExitEffect(agentEntity);
            }
        }

        private void TriggerOnExitEffect(AgentEntity agentEntity)
        {
            foreach (Action<AgentEntity> action in _actionsOnTriggerExit)
            {
                action(agentEntity);
            }
        }

        protected float ReturnSizeOverTime(float time, AnimationCurve animationCurve)
        {
            return animationCurve.Evaluate(time);
        }

        private void Deactivate()
        {
            _agentsInside.Clear();
            _stopwatch.Reset();
            gameObject.SetActive(false);
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            AgentEntity agentEntity = other.GetComponent<AgentEntity>();
            
            _agentsInside.Add(agentEntity);

            _actionOnTriggerEnter(agentEntity);
        }

        protected void OnTriggerExit(Collider other)
        {
            AgentEntity agentEntity = other.GetComponent<AgentEntity>();
            
            _agentsInside.Remove(agentEntity);

            _actionOnTriggerExit(agentEntity);
        }
    }
}