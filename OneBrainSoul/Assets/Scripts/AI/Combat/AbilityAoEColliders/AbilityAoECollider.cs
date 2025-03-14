using System;
using System.Collections;
using System.Collections.Generic;
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

        private List<EntityType> _typesAffectedByTheAbility;

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
        private Action<AgentEntity> _actionOnTriggerExit = agentEntity => { };

        private Func<ushort, float, ushort> _actionMovement = (positionCounter, time) => positionCounter;
        private Action<Vector3> _actionSetPosition;

        private List<AbilityAoEPositions> _positionsOverTime = new List<AbilityAoEPositions>();

        private float _abilityDuration;
        private float _tickTimer;

        protected GameObject _childWithParticleSystem;
        
        private readonly Dictionary<AbilityEffectForceType, ForceMode> _forceModes = new Dictionary<AbilityEffectForceType, ForceMode>
        {
            { AbilityEffectForceType.FORCE , ForceMode.Force},  
            { AbilityEffectForceType.IMPULSE , ForceMode.Impulse},  
            { AbilityEffectForceType.ACCELERATION , ForceMode.Acceleration},  
            { AbilityEffectForceType.VELOCITY_CHANGE , ForceMode.VelocityChange}  
        };

        public virtual void SetAbilitySpecs(Transform parentTransform, BasicAbilityComponent basicAbilityComponent, 
            TAreaAbilityComponent areaAbilityComponent, List<EntityType> typesAffectedByTheAbility)
        {
            _parentTransform = parentTransform;

            AbilityAoE abilityAoE = areaAbilityComponent.GetAoE();
            
            _abilityDuration = abilityAoE.duration;
            
            _relativePosition = abilityAoE.relativePositionToCaster;

            _childWithParticleSystem = Instantiate(abilityAoE.objectWithParticleSystem, transform);

            _childWithParticleSystem.transform.localPosition = abilityAoE.relativePositionForParticles;
            
            _typesAffectedByTheAbility = typesAffectedByTheAbility;

            {
                Vector3 direction = areaAbilityComponent.GetAoE().direction;

                _direction = direction == Vector3.zero ? Vector3.forward : direction.normalized;
            }

            _actionAttaching = areaAbilityComponent.GetAoE().isAttachedToCaster
                ? () =>
                {
                    _actionSetPosition = position => transform.localPosition = position;
                    foreach (AbilityAoEPositions abilityPosition in _positionsOverTime)
                    {
                        abilityPosition.RotatePosition(_parentTransform.forward);
                    }
                }
                : () =>
                {
                    _actionSetPosition = position => transform.position = position;
                    transform.parent = null;
                    foreach (AbilityAoEPositions abilityPosition in _positionsOverTime)
                    {
                        abilityPosition.SetWorldPosition(_parentTransform.forward, _parentTransform.position);
                    }
                };

            AbilityMovement abilityMovement = areaAbilityComponent.GetMovement();

            if (abilityMovement.positions.Count != 0)
            {
                _positionsOverTime = new List<AbilityAoEPositions>
                {
                    new AbilityAoEPositions(0, _relativePosition)
                };

                for (int i = 0; i < abilityMovement.positions.Count; i++)
                {
                    _positionsOverTime.Add(new AbilityAoEPositions(abilityMovement.timeBetweenPositions[i],
                        abilityMovement.positions[i]));
                }
                
                _actionMovement = (positionCounter, timer) =>
                {
                    if (positionCounter >= _positionsOverTime.Count - 1)
                    {
                        return positionCounter;
                    }
                    
                    float currentTime = 0;

                    for (int i = 0; i < positionCounter; i++)
                    {
                        currentTime += _positionsOverTime[i].GetTimeToReach();
                    }

                    float nextTimeSpot = currentTime + _positionsOverTime[positionCounter + 1].GetTimeToReach();
                    
                    _actionSetPosition(Vector3.Lerp(_positionsOverTime[positionCounter].GetRotatedPosition(),
                        _positionsOverTime[positionCounter + 1].GetRotatedPosition(), (timer - currentTime) / (nextTimeSpot - currentTime)));
                    
                    if (timer > nextTimeSpot)
                    {
                        positionCounter++;
                    }

                    return positionCounter;
                };
            }

            AbilityTrigger abilityTrigger = basicAbilityComponent.GetTrigger();

            bool comesFromCaster = areaAbilityComponent.GetAoE().duration == 0;

            if (abilityTrigger.hasAnEffectOnStart)
            {
                SetupOnEnterExit(_actionsOnTriggerEnter, basicAbilityComponent.GetEffectTypeOnStart(), 
                    basicAbilityComponent.GetEffectOnStart(), comesFromCaster);

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
                SetupOnStay(basicAbilityComponent.GetEffectTypeOnTheDuration(), basicAbilityComponent.GetEffectOnTheDuration(), comesFromCaster);
            }

            if (!abilityTrigger.hasAnEffectOnEnd)
            {
                return;
            }
            
            SetupOnEnterExit(_actionsOnTriggerExit, basicAbilityComponent.GetEffectTypeOnEnd(), basicAbilityComponent.GetEffectOnEnd(), comesFromCaster);
            
            if (!areaAbilityComponent.DoesItTriggerOnTriggerExit())
            {
                return;
            }
            _actionOnTriggerExit = TriggerOnExitEffect;
        }

        private void SetupOnEnterExit(List<Action<AgentEntity>> actionsList, AbilityEffectOnHealthType abilityEffectOnHealthType, 
            AbilityEffect abilityEffect, bool comesFromCaster)
        {
            if (abilityEffect.hasAnEffectOnHealth)
            {
                switch (abilityEffectOnHealthType)
                {
                    case AbilityEffectOnHealthType.DAMAGE:
                        if (abilityEffect.isEffectOnHealthAttachedToEntity)
                        {
                            AddDamageOverTime(actionsList, abilityEffect.healthModificationValue, abilityEffect.effectOnHealthDuration, comesFromCaster);
                            break;
                        }
                        AddDamage(actionsList, abilityEffect.healthModificationValue, comesFromCaster);
                        break;
                
                    case AbilityEffectOnHealthType.HEAL:
                        if (abilityEffect.isEffectOnHealthAttachedToEntity)
                        {
                            AddHealOverTime(actionsList, abilityEffect.healthModificationValue, 
                                abilityEffect.effectOnHealthDuration, comesFromCaster);
                            break;
                        }
                    
                        AddHeal(actionsList, abilityEffect.healthModificationValue, comesFromCaster);
                        break;
                }
            }
            
            SetupSlowAndForce(actionsList, abilityEffect, comesFromCaster);
        }

        private void SetupOnStay(AbilityEffectOnHealthType abilityEffectOnHealthType, AbilityEffect abilityEffect, bool comesFromCaster)
        {
            if (abilityEffect.hasAnEffectOnHealth)
            {
                switch (abilityEffectOnHealthType)
                {
                    case AbilityEffectOnHealthType.DAMAGE:
                        AddDamage(_actionsOnTriggerStay, abilityEffect.healthModificationValue, comesFromCaster);
                        _tickTimer = GameManager.Instance.GetTimeBetweenDamageTicks();
                        break;
                
                    
                    case AbilityEffectOnHealthType.HEAL:
                        AddHeal(_actionsOnTriggerStay, abilityEffect.healthModificationValue, comesFromCaster);
                        _tickTimer = GameManager.Instance.GetTimeBetweenHealTicks();
                        break;
                }
            }
            
            SetupSlowAndForce(_actionsOnTriggerStay, abilityEffect, comesFromCaster);
        }

        private void SetupSlowAndForce(List<Action<AgentEntity>> actionsList, AbilityEffect abilityEffect, bool comesFromCaster)
        {
            if (abilityEffect.doesSlow)
            {
                if (!abilityEffect.isSlowAttachedToEntity)
                {
                    AddSlow(actionsList, abilityEffect.slowPercent, comesFromCaster);
                    _actionsOnTriggerExit.Insert(0, ReleaseFromSlow);
                }
                else
                {
                    AddSlowOverTime(actionsList, abilityEffect.slowPercent, abilityEffect.slowDuration, 
                        abilityEffect.doesDecreaseOverTime, comesFromCaster);
                }
            }

            if (!abilityEffect.doesApplyAForce)
            {
                return;
            }

            if (abilityEffect.doesForceComesFromCenterOfTheArea)
            {
                AddPushFromCenter(actionsList, abilityEffect.forceDirection, abilityEffect.forceStrength,
                    comesFromCaster, abilityEffect.abilityEffectForceType);
                return;
            }

            AddPushInADirection(actionsList, abilityEffect.forceDirection, abilityEffect.forceStrength, comesFromCaster,
                abilityEffect.abilityEffectForceType);
        }

        private void AddDamage(List<Action<AgentEntity>> actionsList, uint damageValue, bool comesFromCaster)
        {
            //TODO HIT POSITION
            Transform source = transform;

            if (comesFromCaster)
            {
                source = _parentTransform;
            }
            
            actionsList.Add(agentEntity => Damage(agentEntity, damageValue, Vector3.zero, source.position));
        }

        private void Damage(IDamageable agent, uint damageValue, Vector3 hitPosition, Vector3 sourcePosition)
        {
            agent.OnReceiveDamage(damageValue, hitPosition, sourcePosition);
        }

        private void AddDamageOverTime(List<Action<AgentEntity>> actionsList, uint damageValue, float duration, bool comesFromCaster)
        {
            Transform source = transform;
            actionsList.Add(agentEntity => DamageOverTime(agentEntity, damageValue, duration, source.position));
        }

        private void DamageOverTime(IDamageable agent, uint damageValue, float duration, Vector3 sourcePosition)
        {
            agent.OnReceiveDamageOverTime(damageValue, duration, sourcePosition);
        }

        private void AddHeal(List<Action<AgentEntity>> actionsList, uint healValue, bool comesFromCaster)
        {
            Transform source = transform;

            if (comesFromCaster)
            {
                source = _parentTransform;
            }

            actionsList.Add(agentEntity => Heal(agentEntity, healValue, source.position));
        }

        private void Heal(IHealable agent, uint healValue, Vector3 sourcePosition)
        {
            agent.OnReceiveHeal(healValue, sourcePosition);
        }

        private void AddHealOverTime(List<Action<AgentEntity>> actionsList, uint healValue, float duration, bool comesFromCaster)
        {
            Transform source = transform;

            if (comesFromCaster)
            {
                source = _parentTransform;
            }

            actionsList.Add(agentEntity => HealOverTime(agentEntity, healValue, duration, source.position));
        }

        private void HealOverTime(IHealable agent, uint healValue, float duration, Vector3 sourcePosition)
        {
            agent.OnReceiveHealOverTime(healValue, duration, sourcePosition);
        }

        private void AddSlow(List<Action<AgentEntity>> actionsList, uint slowPercent, bool comesFromCaster)
        {
            Transform source = transform;

            if (comesFromCaster)
            {
                source = _parentTransform;
            }

            actionsList.Add(agentEntity => Slow(agentEntity, slowPercent, source.position));
        }

        private void Slow(ISlowable agent, uint slowPercent, Vector3 sourcePosition)
        {
            agent.OnReceiveSlow((uint)gameObject.GetInstanceID(), slowPercent, sourcePosition);
        }

        private void AddSlowOverTime(List<Action<AgentEntity>> actionsList, uint slowPercent, float duration, bool doesDecrease, 
            bool comesFromCaster)
        {
            Transform source = transform;

            if (comesFromCaster)
            {
                source = _parentTransform;
            }
            
            if (doesDecrease)
            {
                actionsList.Add(agentEntity => SlowOverTime(agentEntity, slowPercent, duration, source.position));
                return;
            }
            
            actionsList.Add(agentEntity => DecreasingSlow(agentEntity, slowPercent, duration, source.position));
        }

        private void SlowOverTime(ISlowable agent, uint slowPercent, float duration, Vector3 sourcePosition)
        {
            agent.OnReceiveSlowOverTime((uint)gameObject.GetInstanceID(), slowPercent, duration, sourcePosition);
        }

        private void DecreasingSlow(ISlowable agent, uint slowPercent, float duration, Vector3 sourcePosition)
        {
            agent.OnReceiveDecreasingSlow((uint)gameObject.GetInstanceID(), slowPercent, duration, sourcePosition);
        }

        private void AddPushFromCenter(List<Action<AgentEntity>> actionsList, Vector3 forceDirection, float forceStrength, 
            bool comesFromCaster, AbilityEffectForceType abilityEffectForceType)
        {
            Transform source = transform;

            if (comesFromCaster)
            {
                source = _parentTransform;
            }

            actionsList.Add(agentEntity => PushFromCenter(agentEntity, forceDirection, forceStrength, source.position, _forceModes[abilityEffectForceType]));
        }

        private void PushFromCenter(IPushable agent, Vector3 forceDirection, float forceStrength, 
            Vector3 sourcePosition, ForceMode forceMode)
        {
            agent.OnReceivePushFromCenter(transform.position, forceDirection, forceStrength, sourcePosition, forceMode);
        }

        private void AddPushInADirection(List<Action<AgentEntity>> actionsList, Vector3 forceDirection, float forceStrength, 
            bool comesFromCaster, AbilityEffectForceType abilityEffectForceType)
        {
            Transform source = transform;

            if (comesFromCaster)
            {
                source = _parentTransform;
            }

            actionsList.Add(agentEntity => PushInADirection(agentEntity, forceDirection, forceStrength, source.position, _forceModes[abilityEffectForceType]));
        }

        private void PushInADirection(IPushable agent, Vector3 forceDirection, float forceStrength, 
            Vector3 sourcePosition, ForceMode forceMode)
        {
            agent.OnReceivePushInADirection(transform.forward, forceDirection, forceStrength, sourcePosition, forceMode);
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

            ushort positionCounter = 0;
            
            _actionOnStartTrigger();
            
            timer += Time.deltaTime;

            while (timer < _abilityDuration)
            {
                timer += Time.deltaTime;
                tickTimer += Time.deltaTime;

                _actionResizing(timer);

                positionCounter = _actionMovement(positionCounter, timer);

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
            gameObject.SetActive(false);
        }

        private Vector3 GetLocalPosition()
        {
            return transform.localPosition;
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            AgentEntity agentEntity = other.GetComponent<AgentEntity>();

            if (!agentEntity)
            {
                return;
            }

            bool match = false;

            foreach (EntityType affectedEntityType in _typesAffectedByTheAbility)
            {
                if (agentEntity.GetEntityType() == affectedEntityType)
                {
                    match = true;
                    break;
                }
            }

            if (!match)
            {
                return;
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

            bool match = false;

            foreach (EntityType affectedEntityType in _typesAffectedByTheAbility)
            {
                if (agentEntity.GetEntityType() == affectedEntityType)
                {
                    match = true;
                    break;
                }
            }

            if (!match)
            {
                return;
            }
            
            _agentsInside.Remove(agentEntity);

            _actionOnTriggerExit(agentEntity);
        }
    }
}