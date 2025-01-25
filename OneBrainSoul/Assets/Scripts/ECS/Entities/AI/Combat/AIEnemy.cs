using System;
using System.Collections;
using System.Collections.Generic;
using AI.Combat.AbilityColliders;
using AI.Combat.AbilitySpecs;
using AI.Combat.Contexts;
using AI.Combat.Enemy;
using AI.Combat.Position;
using AI.Combat.ScriptableObjects;
using ECS.Components.AI.Combat;
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

        [SerializeField] protected GameObject _rectangleAttackColliderPrefab;
        [SerializeField] protected GameObject _circleAttackColliderPrefab;

        protected List<AttackComponent> _attackComponents = new List<AttackComponent>();

        protected Dictionary<AttackComponent, AbilityCollider> _attacksColliders =
            new Dictionary<AttackComponent, AbilityCollider>();

        private Coroutine _updateCoroutine;

        protected SurroundingSlots _surroundingSlots;
        protected AgentSlot _agentSlot;

        protected bool _alive = true;
        
        public AgentSlotPosition GetAgentSlotPosition(Vector3 direction, float radius)
        {
            return _surroundingSlots.ReserveSubtendedAngle(GetAgentID(), direction, radius);
        }

        public void ReleaseAgentSlot(uint agentID)
        {
            _surroundingSlots.FreeSubtendedAngle(agentID);
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

        protected void SetRivalRadius(float rivalRadius)
        {
            _context.SetTargetRadius(rivalRadius);
        }

        protected void SetRivalHeight(float rivalHeight)
        {
            _context.SetTargetHeight(rivalHeight);
        }

        public void SetDistanceToRival(float distanceToRival)
        {
            _context.SetDistanceToTarget(distanceToRival);
        }

        protected void SetIsSeeingARival(bool isSeeingARival)
        {
            _context.SetIsSeeingPlayer(isSeeingARival);
        }

        protected void SetHasATarget(bool hasATarget)
        {
            _context.SetHasATarget(hasATarget);
        }

        public void SetIsFighting(bool isFighting)
        {
            _context.SetIsFighting(isFighting);
        }

        public void SetIsAttacking(bool isAttacking)
        {
            _context.SetIsAttacking(isAttacking);
        }

        public void SetIsAirborne(bool isAirborne)
        {
            _context.SetIsAirborne(isAirborne);
        }

        public void SetVectorToRival(Vector3 vectorToRival)
        {
            _context.SetVectorToTarget(vectorToRival);
        }

        protected void SetRivalTransform(Transform rivalTransform)
        {
            _context.SetTargetTransform(rivalTransform);
        }

        protected virtual void Attacking()
        {
            _context.SetIsAttacking(true);
        }

        protected virtual void NotAttacking()
        {
            _context.SetIsAttacking(false);
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

        public abstract void OnReceiveDamage(DamageComponent damageComponent);

        protected virtual void EnemySetup(float radius, AIEnemySpecs aiEnemySpecs)
        {
            InitiateDictionaries();

            _surroundingSlots = new SurroundingSlots(radius + aiEnemySpecs.agentsPositionRadius);

            //InstantiateAttackComponents(aiEnemySpecs.aiAttacks);
            InstantiateAttacksColliders();
        }

        private void InstantiateAttackComponents(List<CombatAgentAbility> attacks)
        {
            foreach (CombatAgentAbility aiAttack in attacks)
            {
                switch (aiAttack.abilityAoEType)
                {
                    case AbilityAoEType.RECTANGLE_AREA:
                        _attackComponents.Add(new RectangleAttackComponent(aiAttack));
                        break;
                    
                    case AbilityAoEType.CIRCLE_AREA:
                        _attackComponents.Add(new CircleAttackComponent(aiAttack));
                        break;
                    
                    case AbilityAoEType.CONE_AREA:
                        _attackComponents.Add(new ConeAttackComponent(aiAttack));
                        break;
                }
            }
        }
        
        private void InstantiateAttacksColliders()
        {
            int layerTarget = GameManager.Instance.GetAllyLayer();
            
            foreach (AttackComponent attackComponent in _attackComponents)
            {
                GameObject colliderObject = null;

                switch (attackComponent.GetAIAttackAoEType())
                {
                    case AbilityAoEType.RECTANGLE_AREA:
                        colliderObject = Instantiate(_rectangleAttackColliderPrefab);
                        AIEnemyRectangleAbilityCollider rectangleAbilityCollider = 
                            colliderObject.GetComponent<AIEnemyRectangleAbilityCollider>();
                        
                        rectangleAbilityCollider.SetOwner(GetAgentID());
                        rectangleAbilityCollider.SetRectangleAttackComponent((RectangleAttackComponent)attackComponent);
                        rectangleAbilityCollider.SetAbilityTargets((int)Mathf.Pow(2, layerTarget));
                        _attacksColliders.Add(attackComponent, rectangleAbilityCollider);
                        break;

                    case AbilityAoEType.CIRCLE_AREA:
                        colliderObject = Instantiate(_circleAttackColliderPrefab);
                        AIEnemyCircleAbilityCollider circleAbilityCollider = 
                            colliderObject.GetComponent<AIEnemyCircleAbilityCollider>();
                        
                        circleAbilityCollider.SetOwner(GetAgentID());
                        circleAbilityCollider.SetCircleAbilityComponent((CircleAttackComponent)attackComponent);
                        circleAbilityCollider.SetAbilityTargets((int)Mathf.Pow(2, layerTarget));
                        _attacksColliders.Add(attackComponent, circleAbilityCollider);
                        break;

                    case AbilityAoEType.CONE_AREA:
                        AIEnemyConeAbilityCollider coneAbilityCollider = 
                            colliderObject.GetComponent<AIEnemyConeAbilityCollider>();
                        
                        coneAbilityCollider.SetOwner(GetAgentID());
                        coneAbilityCollider.SetConeAbilityComponent((ConeAttackComponent)attackComponent);
                        coneAbilityCollider.SetAbilityTargets((int)Mathf.Pow(2, layerTarget));
                        _attacksColliders.Add(attackComponent, coneAbilityCollider);
                        break;
                }

                colliderObject.SetActive(false);
            }
        }

        #region Attacks Managing

        #region Own Attacks

        protected abstract void StartCastingAbility(AttackComponent attackComponent);

        protected abstract IEnumerator StartAbilityCastTimeCoroutine(AttackComponent attackComponent,
            AbilityCollider abilityCollider);

        protected abstract void PutAbilityOnCooldown(AttackComponent attackComponent);

        protected abstract IEnumerator StartCooldownCoroutine(AttackComponent attackComponent);

        #endregion

        #region Rival Attacks

        /*public void OnReceiveDamage(DamageComponent damageComponent)
        {
            SetHealth(_context.GetHealth() - damageComponent.GetDamage());

            uint health = _context.GetHealth();

            if (health != 0)
            {
                return;
            }

            Destroy(gameObject);
        }*/

        protected virtual void OnDestroy()
        {
            if (EventsManager.OnAgentDefeated != null)
            {
                EventsManager.OnAgentDefeated(GetAgentID());
            }
            
            _alive = false;
        }

        #endregion

        #endregion

        public AIEnemyType GetAIEnemyType()
        {
            return _context.GetEnemyType();
        }

        public List<AttackComponent> GetAttackComponents()
        {
            return _attackComponents;
        }
    }
}