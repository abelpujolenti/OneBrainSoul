using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using AI.Combat.AbilitySpecs;
using ECS.Components.AI.Combat.Abilities;
using ECS.Entities;
using ECS.Entities.AI;
using Interfaces.AI.Combat;
using Managers;
using UnityEngine;

namespace AI.Combat.AbilityAoEColliders
{
    public abstract class AbilityAoECollider<TAreaAbilityComponent> : MonoBehaviour, IAbilityCollider 
        where TAreaAbilityComponent : AreaAbilityComponent
    {
        private Coroutine _abilityDurationCoroutine;

        private Transform _parentTransform;
        
        private Stopwatch _stopwatch = new Stopwatch();

        protected List<EntityType> _abilityTargets;
        
        private List<AgentEntity> _agentsInside = new List<AgentEntity>();

        private List<Action<AgentEntity>> _actionsOnTriggerEnter = new List<Action<AgentEntity>>();
        private List<Action<AgentEntity>> _actionsOnTriggerStay = new List<Action<AgentEntity>>();
        private List<Action<AgentEntity>> _actionsOnTriggerExit = new List<Action<AgentEntity>>();

        private Vector3 _relativePosition;
        
        private Quaternion _parentRotation;

        private Vector3 _direction;

        private Action _actionAttaching = () => { };
        private Action _actionOnStartTrigger = () => { };
        private Action<AgentEntity> _actionOnTriggerEnter = agentEntity => { };
        protected Action<float> _actionResizing = value => { };
        private Action<AgentEntity> _actionOnTriggerExit  = agentEntity => { };

        private float _abilityDuration;
        private float _tickTimer;

        protected virtual void OnDisable()
        {
            _stopwatch.Reset();
        }

        public virtual void SetAbilitySpecs(Transform parentTransform, BasicAbilityComponent basicAbilityComponent, 
            TAreaAbilityComponent areaAbilityComponent)
        {
            _parentTransform = parentTransform;
            
            _abilityDuration = areaAbilityComponent.GetAoE().duration;
            
            _relativePosition = areaAbilityComponent.GetAoE().relativePositionToCaster;

            {
                Vector3 direction = areaAbilityComponent.GetAoE().direction;

                _direction = direction == Vector3.zero ? Vector3.forward : direction.normalized;
            }

            _actionAttaching = areaAbilityComponent.GetAoE().isAttachedToCaster
                ? () => { }
                : () =>
                {
                    transform.parent = null;
                };

            AbilityTrigger abilityTrigger = basicAbilityComponent.GetTrigger();

            if (abilityTrigger.hasAnEffectOnStart)
            {
                SetupOnEnterExit(_actionsOnTriggerEnter, basicAbilityComponent.GetEffectTypeOnStart(), basicAbilityComponent.GetEffectOnStart());

                if (areaAbilityComponent.DoesItTriggerOnTriggerEnter())
                {
                    _actionOnTriggerEnter = TriggerOnEnterEffect;
                }
                else
                {
                    _actionOnStartTrigger = TriggerOnEnterEffect;
                }
            }

            if (abilityTrigger.hasAnEffectOnTheDuration)
            {
                SetupOnStay(basicAbilityComponent.GetEffectTypeOnTheDuration(), basicAbilityComponent.GetEffectOnTheDuration());
            }

            if (!abilityTrigger.hasAnEffectOnEnd)
            {
                return;
            }
            
            SetupOnEnterExit(_actionsOnTriggerExit, basicAbilityComponent.GetEffectTypeOnEnd(), basicAbilityComponent.GetEffectOnEnd());
            
            if (!areaAbilityComponent.DoesItTriggerOnTriggerExit())
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
                    _actionsOnTriggerExit.Insert(0, ReleaseFromSlow);
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

            if (abilityEffect.doesForceComesFromCenterOfTheArea)
            {
                AddPushFromCenter(actionsList, abilityEffect.forceDirection, abilityEffect.forceStrength);
                return;
            }
            AddPushInADirection(actionsList, abilityEffect.forceDirection, abilityEffect.forceStrength);
        }

        public void SetAbilityTargets(List<EntityType> abilityTargets)
        {
            _abilityTargets = abilityTargets;
        }

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
            agent.OnReceiveSlow((uint)gameObject.GetInstanceID(), slowPercent);
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
            agent.OnReceiveSlowOverTime((uint)gameObject.GetInstanceID(), slowPercent, duration);
        }

        private void DecreasingSlow(ISlowable agent, uint slowPercent, float duration)
        {
            agent.OnReceiveDecreasingSlow((uint)gameObject.GetInstanceID(), slowPercent, duration);
        }

        private void AddPushFromCenter(List<Action<AgentEntity>> actionsList, Vector3 forceDirection, float forceStrength)
        {
            actionsList.Add(agentEntity => { PushFromCenter(agentEntity, forceDirection, forceStrength); });
        }

        private void PushFromCenter(IPushable agent, Vector3 forceDirection, float forceStrength)
        {
            agent.OnReceivePushFromCenter(transform.position, forceDirection, forceStrength);
        }

        private void AddPushInADirection(List<Action<AgentEntity>> actionsList, Vector3 forceDirection, float forceStrength)
        {
            actionsList.Add(agentEntity => { PushInADirection(agentEntity, forceDirection, forceStrength); });
        }

        private void PushInADirection(IPushable agent, Vector3 forceDirection, float forceStrength)
        {
            agent.OnReceivePushInADirection(transform.forward, forceDirection, forceStrength);
        }

        private void AddReleaseFromSlow(List<Action<AgentEntity>> actionsList)
        {
            actionsList.Add(ReleaseFromSlow);
        }

        private void ReleaseFromSlow(ISlowable agent)
        {
            agent.OnReleaseFromSlow((uint)gameObject.GetInstanceID());
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
            gameObject.SetActive(true);
            _stopwatch.Start();
            _actionResizing(0);
            
            SetParent(_parentTransform);
            MoveToPosition(_relativePosition);
            _parentRotation = _parentTransform.rotation;
            Rotate();
            
            _actionAttaching();
            
            _abilityDurationCoroutine = StartCoroutine(AbilityDurationCoroutine());
        }

        private void Rotate()
        {
            transform.rotation = _parentRotation * Quaternion.LookRotation(_direction, Vector3.up);
        }

        private IEnumerator AbilityDurationCoroutine()
        {
            float timer = 0;
            float tickTimer = 0;

            yield return null;
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

            if (!agentEntity)
            {
                return;
            }

            for (int i = 0; i < _abilityTargets.Count; i++)
            {
                if (agentEntity.GetEntityType() == _abilityTargets[i])
                {
                    break;
                }

                if (i == _abilityTargets.Count - 1)
                {
                    return;
                }
            }
            
            _agentsInside.Add(agentEntity);

            _actionOnTriggerEnter(agentEntity);
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            AgentEntity agentEntity = other.GetComponent<AgentEntity>();

            if (!agentEntity)
            {
                return;
            }

            for (int i = 0; i < _abilityTargets.Count; i++)
            {
                if (agentEntity.GetEntityType() == _abilityTargets[i])
                {
                    break;
                }

                if (i == _abilityTargets.Count - 1)
                {
                    return;
                }
            }
            
            _agentsInside.Remove(agentEntity);

            _actionOnTriggerExit(agentEntity);
        }
    }
}