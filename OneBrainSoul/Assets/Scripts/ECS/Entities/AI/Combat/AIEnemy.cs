using System;
using System.Collections;
using System.Collections.Generic;
using AI;
using AI.Combat;
using AI.Combat.Ally;
using AI.Combat.Enemy;
using AI.Combat.Position;
using AI.Combat.ScriptableObjects;
using ECS.Components.AI.Combat;
using Managers;
using UnityEngine;

namespace ECS.Entities.AI.Combat
{
    public class AIEnemy : AICombatAgentEntity<AIEnemyContext, AttackComponent, AllyDamageComponent, AIEnemyAction>
    {
        [SerializeField] private AIEnemySpecs _aiEnemySpecs;
        
        private void Start()
        {
            _utilityFunction = new AIEnemyUtilityFunction();
            InitiateDictionaries();
            Setup();
            InstantiateAttackComponents(_aiEnemySpecs.aiAttacks);
            CalculateMinimumAndMaximumRangeToAttacks(_attackComponents);

            _raysTargetsLayerMask = (int)(Math.Pow(2, GameManager.Instance.GetEnemyLayer()) + 
                                          Math.Pow(2, GameManager.Instance.GetGroundLayer()));

            CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();

            float radius = capsuleCollider.radius;

            _surroundingSlots = new SurroundingSlots(radius + _aiEnemySpecs.rivalsPositionRadius);
            
            _context = new AIEnemyContext(_aiEnemySpecs.totalHealth, radius, 
                _aiEnemySpecs.sightMaximumDistance, _minimumRangeToCastAnAttack, _maximumRangeToCastAnAttack, transform, 
                _aiEnemySpecs.maximumStress, _aiEnemySpecs.stunDuration);
            
            CombatManager.Instance.AddAIEnemy(this);
            ECSNavigationManager.Instance.AddNavMeshAgentEntity(GetAgentID(), GetNavMeshAgentComponent(), radius);
            
            StartUpdate();
        }

        protected override void InitiateDictionaries()
        {
            _actions = new Dictionary<AIEnemyAction, Action>
            {
                { AIEnemyAction.PATROL , Patrol},
                { AIEnemyAction.CHOOSE_NEW_RIVAL , RequestRival},
                { AIEnemyAction.GET_CLOSER_TO_RIVAL , GetCloserToRival},
                { AIEnemyAction.ROTATE , Rotate},
                { AIEnemyAction.ATTACK , Attack},
                { AIEnemyAction.FLEE , Flee}
            };
        }

        private void InstantiateAttackComponents(List<AIAttack> attacks)
        {
            foreach (AIAttack aiAttack in attacks)
            {
                switch (aiAttack.aiAttackAoEType)
                {
                    case AIAttackAoEType.RECTANGLE_AREA:
                        _attackComponents.Add(new RectangleAttackComponent(aiAttack));
                        break;
                    
                    case AIAttackAoEType.CIRCLE_AREA:
                        _attackComponents.Add(new CircleAttackComponent(aiAttack));
                        break;
                    
                    case AIAttackAoEType.CONE_AREA:
                        _attackComponents.Add(new ConeAttackComponent(aiAttack));
                        break;
                }
            }
        }

        #region AI Loop

        protected override IEnumerator UpdateCoroutine()
        {
            while (true)
            {
                UpdateVisibleRivals();

                UpdateVectorToRival();

                if (_context.IsAttacking())
                {
                    yield return null;
                    continue;
                }
                
                LaunchRaycasts();
            
                CalculateBestAction();

                if (!_context.HasATarget())
                {
                    yield return null;
                    continue;
                }
                
                RivalSlotPosition rivalSlotPosition = CombatManager.Instance.RequestAlly(_context.GetRivalID())
                    .GetRivalSlotPosition(_context.GetVectorToRival(), _context.GetRadius());

                if (rivalSlotPosition == null)
                {
                    yield return null;
                    continue;
                }

                _rivalSlot = rivalSlotPosition.rivalSlot;
                ECSNavigationManager.Instance.UpdateAStarDeviationVector(GetAgentID(), rivalSlotPosition.deviationVector);

                yield return null;
            }
        }

        protected override void UpdateVisibleRivals()
        {
            _visibleRivals = CombatManager.Instance.GetVisibleRivals<AIAlly, AIAllyContext, AllyAttackComponent, 
                DamageComponent, AIAllyAction, AIEnemyContext, AttackComponent, AllyDamageComponent, AIEnemyAction>(this);

            _context.SetIsSeeingARival(_visibleRivals.Count != 0);
        }

        #endregion

        #region FSM

        private void Patrol()
        {
            ShowActionDebugLogs(name + " Patrolling");
            //TODO
        }

        private void RequestRival()
        {
            ShowActionDebugLogs(name + " Requesting Rival");
            
            if (_visibleRivals.Count == 0)
            {
                return;
            }

            uint targetID = ObtainTargetID(_visibleRivals);
            
            OnTargetAcquired(targetID, CombatManager.Instance.RequestAlly(targetID).GetContext());
        }

        private uint ObtainTargetID(List<uint> possibleRivals)
        {
            uint targetID;

            if (possibleRivals.Count == 1)
            {
                targetID = possibleRivals[0];
            }
            else
            {
                targetID = CombatManager.Instance.GetClosestRivalID<AIAlly, AIAllyContext, AllyAttackComponent,
                    DamageComponent, AIAllyAction>(GetNavMeshAgentComponent().GetTransformComponent(),
                    possibleRivals, AIAgentType.ALLY);
            }

            return targetID;
        }

        private void OnTargetAcquired(uint enemyID, AIAllyContext allyContext)
        {
            SetRivalIndex(enemyID);
            SetRivalRadius(allyContext.GetRadius());
            SetHasATarget(true);
            SetRivalTransform(allyContext.GetAgentTransform());
        }

        private void GetCloserToRival()
        {
            ShowActionDebugLogs(name + " Getting Closer To Rival");
            
            ContinueNavigation();
            
            SetDestination(CombatManager.Instance.RequestAlly(_context.GetRivalID())
                .GetNavMeshAgentComponent().GetTransformComponent());
        }

        private void Attack()
        {
            StopNavigation();

            AttackComponent attackComponent = ReturnNextAttack();
            
            Attacking();
            
            CombatManager.Instance.EnemyStartCastingAnAttack(transform, attackComponent, this);
        }

        private void Flee()
        {
            ContinueNavigation();
        }

        #endregion

        public override void OnReceiveDamage(AllyDamageComponent damageComponent)
        {
            _context.SetHealth(_context.GetHealth() - damageComponent.GetDamage());

            uint health = _context.GetHealth();

            if (health == 0)
            {
                OnDefeated();
                return;
            }

            uint combatAgentInstanceID = GetAgentID();
            
            bool isStunned = _context.IsStunned();

            if (!isStunned)
            {
                _context.SetCurrentStress(_context.GetCurrentStress() + damageComponent.GetStressDamage());
                isStunned = _context.IsStunned();
                
                if (isStunned)
                {
                    StartCoroutine(StunDuration());
                }
                else
                {
                    //TODO FEEDBACK
                }
                
                CombatManager.Instance.OnEnemyReceiveDamage(combatAgentInstanceID, health, _context.GetCurrentStress(), isStunned);
                return;
            }
            
            CombatManager.Instance.OnEnemyReceiveDamage(combatAgentInstanceID, health, _context.GetCurrentStress(), true);
        }

        protected override void OnDefeated()
        {
            CombatManager.Instance.OnEnemyDefeated(this);
            ECSNavigationManager.Instance.RemoveNavMeshAgentEntity(GetAgentID(), true);
            Destroy(gameObject);
        }

        private IEnumerator StunDuration()
        {
            float stunDuration = GetContext().GetStunDuration();
            float time = 0;

            GetNavMeshAgentComponent().GetNavMeshAgent().isStopped = true;
            
            while (time < stunDuration)
            {
                time += Time.deltaTime;
                yield return null;
            }

            GetNavMeshAgentComponent().GetNavMeshAgent().isStopped = false;
            
            GetContext().SetIsStunned(false);
            
            RotateToNextPathCorner();
            CombatManager.Instance.OnEnemyStunEnds(GetAgentID());
        }

        public override AIAgentType GetAIAgentType()
        {
            return _aiEnemySpecs.aiAgentType;
        }

        public override AIEnemyContext GetContext()
        {
            return _context;
        }

        public void SetCurrentStress(float currentStress)
        {
            _context.SetCurrentStress(currentStress);
        }

        public List<AttackComponent> GetAttackComponents()
        {
            return _attackComponents;
        }
    }
}
