using System;
using System.Collections;
using System.Collections.Generic;
using AI;
using AI.Combat;
using AI.Combat.Ally;
using AI.Combat.AttackColliders;
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

        private bool _alive = true;
        
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
            
            _context = new AIEnemyContext(_aiEnemySpecs.totalHealth, radius, capsuleCollider.height, 
                _aiEnemySpecs.sightMaximumDistance, _minimumRangeToCastAnAttack, _maximumRangeToCastAnAttack, transform, 
                _aiEnemySpecs.maximumStress, _aiEnemySpecs.stunDuration);
            
            CombatManager.Instance.AddAIEnemy(this);
            ECSNavigationManager.Instance.AddNavMeshAgentEntity(GetAgentID(), GetNavMeshAgentComponent(), radius);

            InstantiateAttacksColliders();
            
            StartUpdate();
        }

        protected override void InitiateDictionaries()
        {
            _actions = new Dictionary<AIEnemyAction, Action>
            {
                { AIEnemyAction.PATROL , Patrol },
                { AIEnemyAction.CHOOSE_NEW_RIVAL , RequestRival },
                { AIEnemyAction.GET_CLOSER_TO_RIVAL , GetCloserToRival },
                { AIEnemyAction.ROTATE , Rotate },
                { AIEnemyAction.ATTACK , Attack },
                { AIEnemyAction.FLEE , Flee }
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
        
        private void InstantiateAttacksColliders()
        {
            int layerTarget = GameManager.Instance.GetAllyLayer();
            
            foreach (AttackComponent attackComponent in _attackComponents)
            {
                GameObject colliderObject = null;

                switch (attackComponent.GetAIAttackAoEType())
                {
                    case AIAttackAoEType.RECTANGLE_AREA:
                        colliderObject = Instantiate(_rectangleAttackColliderPrefab);
                        AIEnemyRectangleAttackCollider rectangleAttackCollider = 
                            colliderObject.GetComponent<AIEnemyRectangleAttackCollider>();
                        
                        rectangleAttackCollider.SetOwner(GetAgentID());
                        rectangleAttackCollider.SetRectangleAttackComponent((RectangleAttackComponent)attackComponent);
                        rectangleAttackCollider.SetAttackTargets((int)Mathf.Pow(2, layerTarget));
                        _attacksColliders.Add(attackComponent, rectangleAttackCollider);
                        break;

                    case AIAttackAoEType.CIRCLE_AREA:
                        colliderObject = Instantiate(_circleAttackColliderPrefab);
                        AIEnemyCircleAttackCollider circleAttackCollider = 
                            colliderObject.GetComponent<AIEnemyCircleAttackCollider>();
                        
                        circleAttackCollider.SetOwner(GetAgentID());
                        circleAttackCollider.SetCircleAttackComponent((CircleAttackComponent)attackComponent);
                        circleAttackCollider.SetAttackTargets((int)Mathf.Pow(2, layerTarget));
                        _attacksColliders.Add(attackComponent, circleAttackCollider);
                        break;

                    case AIAttackAoEType.CONE_AREA:
                        AIEnemyConeAttackCollider coneAttackCollider = 
                            colliderObject.GetComponent<AIEnemyConeAttackCollider>();
                        
                        coneAttackCollider.SetOwner(GetAgentID());
                        coneAttackCollider.SetConeAttackComponent((ConeAttackComponent)attackComponent);
                        coneAttackCollider.SetAttackTargets((int)Mathf.Pow(2, layerTarget));
                        _attacksColliders.Add(attackComponent, coneAttackCollider);
                        break;
                }

                colliderObject.SetActive(false);
            }
        }

        #region AI Loop

        protected override IEnumerator UpdateCoroutine()
        {
            while (_alive)
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
                
                AgentSlotPosition agentSlotPosition = CombatManager.Instance.RequestAlly(_context.GetRivalID())
                    .GetAgentSlotPosition(_context.GetVectorToRival(), _context.GetRadius());

                if (agentSlotPosition == null)
                {
                    yield return null;
                    continue;
                }

                _agentSlot = agentSlotPosition.agentSlot;
                ECSNavigationManager.Instance.UpdateAStarDeviationVector(GetAgentID(), agentSlotPosition.deviationVector);

                yield return null;
            }
        }

        protected override void UpdateVisibleRivals()
        {
            _visibleRivals = CombatManager.Instance.GetVisibleRivals<AIAlly, AIAllyContext, AllyAttackComponent, 
                DamageComponent, AIAllyAction, AIEnemyContext, AttackComponent, AllyDamageComponent, AIEnemyAction>(this);

            SetIsSeeingARival(_visibleRivals.Count != 0);
        }

        #endregion

        #region FSM

        private void Patrol()
        {
            ShowActionDebugLogs(name + " Patrolling");
            //TODO
        }

        protected override void RequestRival()
        {
            ShowActionDebugLogs(name + " Requesting Rival");
            
            if (_visibleRivals.Count == 0)
            {
                return;
            }

            AIAlly previousAlly = CombatManager.Instance.RequestAlly(_context.GetRivalID());

            if (previousAlly != null)
            {
                previousAlly.ReleaseAgentSlot(GetAgentID());
            }

            uint targetID = ObtainTargetID(_visibleRivals);
            
            _context.SetIsDueling(false);
            
            OnTargetAcquired(targetID, CombatManager.Instance.RequestAlly(targetID).GetContext());
        }

        protected override uint ObtainTargetID(List<uint> possibleRivals)
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

        private void OnTargetAcquired(uint allyID, AIAllyContext allyContext)
        {
            SetRivalIndex(allyID);
            SetRivalRadius(allyContext.GetRadius());
            SetRivalHeight(allyContext.GetHeight());
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
            ShowActionDebugLogs(name + " Attacking");
            
            StopNavigation();

            AttackComponent attackComponent = ReturnNextAttack();
            
            Attacking();
            
            StartCastingAttack(attackComponent);
        }

        private void Flee()
        {
            ShowActionDebugLogs(name + " Fleeing");
            
            ContinueNavigation();
        }

        #endregion

        #region Attacks Managing

        #region Own Attacks

        protected override void StartCastingAttack(AttackComponent attackComponent)
        {
            if (attackComponent.IsOnCooldown())
            {
                NotAttacking();
                return;
            }
            
            AIAttackCollider attackCollider = _attacksColliders[attackComponent];
            attackCollider.SetParent(transform);
            attackCollider.gameObject.SetActive(true);
            StartCoroutine(StartAttackCastTimeCoroutine(attackComponent, attackCollider));
        }

        protected override IEnumerator StartAttackCastTimeCoroutine(AttackComponent attackComponent, AIAttackCollider attackCollider)
        {
            attackComponent.StartCastTime();

            bool isStunned = _context.IsStunned();
            
            while (attackComponent.IsCasting() && !isStunned)
            {
                attackComponent.DecreaseCurrentCastTime();
                yield return null;
            }

            if (!isStunned)
            {
                attackCollider.StartInflictingDamage();

                if (attackComponent.DoesDamageOverTime())
                {
                    StartCoroutine(StartDamageOverTime(attackComponent, attackCollider));
                    yield break;
                }
            
                Rotate();
            }
            
            PutAttackOnCooldown(attackComponent);
            attackCollider.Deactivate();
        }

        protected override IEnumerator StartDamageOverTime(AttackComponent attackComponent, AIAttackCollider attackCollider)
        {
            while (attackComponent.DidDamageOverTimeFinished())
            {
                attackComponent.DecreaseRemainingTimeDealingDamage();
                yield return null;
            }
           
            Rotate();
            PutAttackOnCooldown(attackComponent);
            attackCollider.Deactivate();
        }

        protected override void PutAttackOnCooldown(AttackComponent attackComponent)
        {
            NotAttacking();
            StartCoroutine(StartCooldownCoroutine(attackComponent));
        }

        protected override IEnumerator StartCooldownCoroutine(AttackComponent attackComponent)
        {
            attackComponent.StartCooldown();
            while (attackComponent.IsOnCooldown())
            {
                attackComponent.DecreaseCooldown();
                yield return null;
            }
            
            OnAttackAvailableAgain(attackComponent);
        }

        #endregion

        #region Rival Attacks

        public void RequestDuel(uint agentID, AIAllyContext allyContext)
        {
            if (_context.IsDueling())
            {
                return;
            }
            
            _context.SetIsDueling(true);
            
            if (agentID == _context.GetRivalID())
            {
                return;
            }
            
            OnTargetAcquired(agentID, allyContext);
        }

        public override void OnReceiveDamage(AllyDamageComponent damageComponent)
        {
            SetHealth(_context.GetHealth() - damageComponent.GetDamage());

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
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (EventsManager.OnAgentDefeated != null)
            {
                EventsManager.OnAgentDefeated(GetAgentID());
            }
            
            CombatManager.Instance.OnEnemyDefeated(this);
            ECSNavigationManager.Instance.RemoveNavMeshAgentEntity(GetAgentID(), true);
            _alive = false;
        }

        #endregion

        #endregion

        #region Context

        public void SetIsDueling(bool isDueling)
        {
            _context.SetIsDueling(isDueling);
        }

        #endregion

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
            
            Rotate();
            
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

        public List<AttackComponent> GetAttackComponents()
        {
            return _attackComponents;
        }
    }
}
