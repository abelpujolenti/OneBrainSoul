using System;
using System.Collections;
using System.Collections.Generic;
using AI.Combat.AbilityAoEColliders;
using AI.Combat.AbilitySpecs;
using AI.Combat.Contexts;
using AI.Combat.Enemy;
using AI.Combat.ScriptableObjects;
using ECS.Components.AI.Combat.Abilities;
using Interfaces.AI.UBS.BaseInterfaces.Get;
using Managers;
using UnityEngine;

namespace ECS.Entities.AI.Combat
{
    public abstract class AIEnemy<TContext, TAction> : AgentEntity
        where TContext : AIEnemyContext
        where TAction : Enum
    {
        protected TContext _context;

        protected IGetBestAction<TAction, TContext> _utilityFunction;

        protected Dictionary<TAction, Action> _actions;

        private Coroutine _updateCoroutine;

        protected virtual void EnemySetup(float radius, AIEnemyProperties aiEnemyProperties)
        {
            _receiveDamageCooldown = GameManager.Instance.GetEnemyReceiveDamageCooldown();
            
            Setup(radius + aiEnemyProperties.agentsPositionRadius);
            
            InitiateDictionaries();
            
            CreateAbilities();
        }

        protected abstract void CreateAbilities();

        protected TAbilityCollider InstantiateAbilityCollider<TAbilityComponent, TAbilityCollider>
            (TAbilityComponent abilityComponent)
                where TAbilityCollider : AbilityAoECollider<TAbilityComponent>
                where TAbilityComponent : AbilityComponent
        {
            GameObject colliderObject = Instantiate(ReturnPrefab(abilityComponent.GetAoEType()));
            
            TAbilityCollider abilityCollider = colliderObject.GetComponent<TAbilityCollider>();
            
            abilityCollider.SetAbilitySpecs(transform, abilityComponent);

            switch (abilityComponent.GetTarget())
            {
                case AbilityTarget.PLAYER:
                    abilityCollider.SetAbilityTargets(GameManager.Instance.GetEntityTypeLayer(EntityType.PLAYER));
                    break;
                
                case AbilityTarget.TRIFACE:
                    abilityCollider.SetAbilityTargets(GameManager.Instance.GetEntityTypeLayer(EntityType.TRIFACE));
                    break;
                
                case AbilityTarget.LONG_ARMS:
                    abilityCollider.SetAbilityTargets(GameManager.Instance.GetEntityTypeLayer(EntityType.LONG_ARMS));
                    break;
                
                case AbilityTarget.LONG_ARMS_BASE:
                    abilityCollider.SetAbilityTargets(GameManager.Instance.GetEntityTypeLayer(EntityType.LONG_ARMS_BASE));
                    break;
                
                case AbilityTarget.OTHER_ENEMY:
                    abilityCollider.SetAbilityTargets(GameManager.Instance.GetEnemyLayer());
                    break;
                
                case AbilityTarget.OTHER_ENEMY_EQUAL_OF_MY_TYPE:
                    abilityCollider.SetAbilityTargets(GameManager.Instance.GetEntityTypeLayer(_entityType));
                    break;
                
                case AbilityTarget.OTHER_ENEMY_DIFFERENT_FROM_MY_TYPE:
                    abilityCollider.SetAbilityTargets(GameManager.Instance.GetDifferentEnemiesLayerFromMyType(_entityType));
                    break;
            }
            
            colliderObject.SetActive(false);

            return abilityCollider;
        }

        private GameObject ReturnPrefab(AbilityAoEType abilityAoEType)
        {
            switch (abilityAoEType)
            {
                case AbilityAoEType.RECTANGULAR:
                    return CombatManager.Instance.GetRectanglePrefab();
                
                case AbilityAoEType.SPHERICAL:
                    return CombatManager.Instance.GetCirclePrefab();
                
                case AbilityAoEType.CONICAL:
                    //return _coneAbilityColliderPrefab;
                    break;
            }

            return null;
        }
        
        #region Context
        
        public abstract TContext GetContext();

        private void SetLastActionIndex(uint lastActionIndex)
        {
            _context.SetLastActionIndex(lastActionIndex);
        }

        protected void SetHealth(uint health)
        {
            _context.SetHealth(health);
        }

        protected void SetTargetRadius(float rivalRadius)
        {
            _context.SetTargetRadius(rivalRadius);
        }

        protected void SetTargetHeight(float rivalHeight)
        {
            _context.SetTargetHeight(rivalHeight);
        }

        public void SetDistanceToTarget(float distanceToRival)
        {
            _context.SetDistanceToTarget(distanceToRival);
        }

        protected void SetIsSeeingATarget(bool isSeeingATarget)
        {
            _context.SetIsSeeingATarget(isSeeingATarget);
        }

        protected void SetHasATarget(bool hasATarget)
        {
            _context.SetHasATarget(hasATarget);
        }

        public void SetIsFighting(bool isFighting)
        {
            _context.SetIsFighting(isFighting);
        }

        public void SetIsCastingAnAbility(bool isAttacking)
        {
            _context.SetICastingAnAbility(isAttacking);
        }

        public void SetIsAirborne(bool isAirborne)
        {
            _context.SetIsAirborne(isAirborne);
        }

        public void SetVectorToRival(Vector3 vectorToRival)
        {
            _context.SetVectorToTarget(vectorToRival);
        }

        protected void SetTargetTransform(Transform rivalTransform)
        {
            _context.SetTargetTransform(rivalTransform);
        }

        protected virtual void CastingAnAbility()
        {
            _context.SetICastingAnAbility(true);
        }

        protected virtual void NotCastingAnAbility()
        {
            _context.SetICastingAnAbility(false);
        }

        protected void UpdateVectorToTarget()
        {
            if (!_context.HasATarget())
            {
                return;
            }

            Vector3 rivalPosition = _context.GetTargetTransform().position;
            Vector3 agentPosition = transform.position;
            
            rivalPosition.y -= _context.GetTargetHeight() / 2;
            agentPosition.y -= _context.GetHeight() / 2;
            
            _context.SetVectorToTarget(rivalPosition - agentPosition);
        }

        #endregion
        
        #region UBS

        protected abstract void InitiateDictionaries();

        protected void CalculateBestAction()
        {
            CheckIfCanPerformGivenAction(_utilityFunction.GetBestAction(_context));
        }

        private void CheckIfCanPerformGivenAction(TAction action)
        {
            uint agentActionUInt = Convert.ToUInt16(action);
            uint lastAction = _context.GetLastActionIndex();

            List<uint> repeatableActions = _context.GetRepeatableActions();

            if (agentActionUInt == lastAction && !repeatableActions.Contains(lastAction))
            {
                return;
            }
            
            SetLastActionIndex(agentActionUInt);

            _actions[action]();
        }

        #endregion
        
        #region FSM

        protected abstract void UpdateVisibleTargets();

        #endregion

        #region Abilities Managing

        #region Own Abilities

        protected abstract void PutAbilityOnCooldown(AbilityComponent abilityComponent);

        protected abstract IEnumerator StartCooldownCoroutine(AbilityComponent abilityComponent);

        #endregion

        #region Ally Abilities

        public override void OnReceiveHeal(uint healValue)
        {
            SetHealth(_context.GetHealth() + healValue);
        }

        public override void OnReceiveHealOverTime(uint healValue, float duration)
        {
            //TODO ENEMY HEAL OVER TIME
        }

        #endregion

        #region Rival Abilities

        public override void OnReceiveDamage(uint damageValue, Vector3 hitPosition)
        {
            if (_currentReceiveDamageCooldown > 0f)
            {
                return;
            }
            
            base.OnReceiveDamage(damageValue, hitPosition);
            
            SetHealth(_context.GetHealth() - damageValue);

            if (_context.GetHealth() != 0)
            {
                StartCoroutine(DecreaseDamageCooldown());
                return;
            }

            Destroy(gameObject);
        }

        public override void OnReceiveSlow(uint slowPercent)
        {
            //TODO ENEMY SLOW
        }

        public override void OnReceiveSlowOverTime(uint slowPercent, float duration)
        {
            //TODO ENEMY SLOW OVER TIME
        }

        public override void OnReceiveDecreasingSlow(uint slowPercent, float duration)
        {
            //TODO ENEMY DECREASING SLOW
        }

        public override void OnReceivePush(Vector3 forceDirection, float forceStrength)
        {
            base.OnReceivePush(forceDirection, forceStrength);
        }

        protected virtual void OnDestroy()
        {
            if (EventsManager.OnAgentDefeated != null)
            {
                EventsManager.OnAgentDefeated(GetAgentID());
            }
            //TODO HEAL PLAYER IF MARKED
        }

        #endregion

        #endregion
        
        ///////////////TODO ERASE
        [SerializeField] private bool _showMessages;

        protected void ShowDebugMessages(string message)
        {
            if (!_showMessages)
            {
                return;
            }
            
            Debug.Log(message);
        }
        ///////////////

        public EnemyType GetAIEnemyType()
        {
            // TODO ?????
            return _context.GetEnemyType();
        }
    }
}