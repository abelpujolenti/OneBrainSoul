using System;
using System.Collections;
using System.Collections.Generic;
using AI;
using AI.Combat;
using AI.Combat.Ally;
using AI.Combat.ScriptableObjects;
using ECS.Components.AI.Combat;
using ECS.Components.AI.Navigation;
using Interfaces.AI.Combat;
using Managers;
using UnityEngine;
using Utilities;

namespace ECS.Entities.AI.Combat
{
    public class AIAlly : AICombatAgentEntity<AIAllyContext, AllyAttackComponent, DamageComponent>
    {
        [SerializeField] private AIAllySpecs _aiAllySpecs;

        private List<uint> _enemiesThatTargetsMe = new List<uint>();

        private List<AIEnemyAttackCollider> _oncomingEnemyAttacks = new List<AIEnemyAttackCollider>();

        private Dictionary<AllyAttackComponent, AIAttackCollider> _attacksColliders =
            new Dictionary<AllyAttackComponent, AIAttackCollider>();
        
        private DieComponent _dieComponent;

        [SerializeField] private bool _isAI;

        private void Start()
        {
            Setup();
            SetupCombatComponents();
            InstantiateAttackComponents(_aiAllySpecs.aiAttacks);
            CalculateMinimumAndMaximumRangeToAttacks(_attackComponents);

            CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
            
            _dieComponent = new DieComponent();
            _context = new AIAllyContext(_aiAllySpecs.totalHealth, capsuleCollider.radius,
                _aiAllySpecs.sightMaximumDistance, _minimumRangeToCastAnAttack, _maximumRangeToCastAnAttack, transform, 
                _navMeshAgent.stoppingDistance, capsuleCollider.height, _aiAllySpecs.alertRadius, _aiAllySpecs.safetyRadius);
            
            CombatManager.Instance.AddAIAlly(this);
            
            InstantiateAttacksColliders();

            if (!_isAI)
            {
                return;
            }
            
            StartUpdate();
        }

        private void InstantiateAttackComponents(List<AIAllyAttack> attacks)
        {
            foreach (AIAllyAttack aiAllyAttack in attacks)
            {
                switch (aiAllyAttack.aiAttackAoEType)
                {
                    case AIAttackAoEType.RECTANGLE_AREA:
                        _attackComponents.Add(new AllyRectangleAttackComponent(GetCombatAgentInstance(), aiAllyAttack));
                        break;
                    
                    case AIAttackAoEType.CIRCLE_AREA:
                        _attackComponents.Add(new AllyCircleAttackComponent(GetCombatAgentInstance(), aiAllyAttack));
                        break;
                    
                    case AIAttackAoEType.CONE_AREA:
                        _attackComponents.Add(new AllyConeAttackComponent(GetCombatAgentInstance(), aiAllyAttack));
                        break;
                }
            }
        }

        private void InstantiateAttacksColliders()
        {
            int layerTarget = GameManager.Instance.GetEnemyLayer();
            
            foreach (AllyAttackComponent attackComponent in _attackComponents)
            {
                GameObject colliderObject = new GameObject();

                switch (attackComponent.GetAIAttackAoEType())
                {
                    case AIAttackAoEType.RECTANGLE_AREA:
                        AIAllyRectangleAttackCollider rectangleAttackCollider =
                            colliderObject.AddComponent<AIAllyRectangleAttackCollider>();
                        
                        rectangleAttackCollider.SetRectangleAttackComponent((AllyRectangleAttackComponent)attackComponent);
                        rectangleAttackCollider.SetAttackTargets((int)Mathf.Pow(2, layerTarget));
                        _attacksColliders.Add(attackComponent, rectangleAttackCollider);
                        break;

                    case AIAttackAoEType.CIRCLE_AREA:
                        AIAllyCircleAttackCollider circleAttackCollider =
                            colliderObject.AddComponent<AIAllyCircleAttackCollider>();
                        
                        circleAttackCollider.SetCircleAttackComponent((AllyCircleAttackComponent)attackComponent);
                        circleAttackCollider.SetAttackTargets((int)Mathf.Pow(2, layerTarget));
                        _attacksColliders.Add(attackComponent, circleAttackCollider);
                        break;

                    case AIAttackAoEType.CONE_AREA:
                        AIAllyConeAttackCollider coneAttackCollider = colliderObject.AddComponent<AIAllyConeAttackCollider>();
                        coneAttackCollider.SetConeAttackComponent((AllyConeAttackComponent)attackComponent);
                        coneAttackCollider.SetAttackTargets((int)Mathf.Pow(2, layerTarget));
                        _attacksColliders.Add(attackComponent, coneAttackCollider);
                        break;
                }

                colliderObject.SetActive(false);
            }
        }

        protected override void StartUpdate()
        {
            base.StartUpdate();
            _isAI = true;
        }

        protected override void StopUpdate()
        {
            base.StopUpdate();
            _isAI = false;
        }

        protected override IEnumerator UpdateCoroutine()
        {
            while (true)
            {
                UpdateVisibleRivals();

                UpdateVectorToRival();

                UpdateDistancesToEnemiesThatTargetsMe();

                if (_context.IsAttacking())
                {
                    yield return null;
                    continue;
                }

                if (_isRotating)
                {
                    yield return null;
                    continue;
                }
                
                //TODO REMAINING DISTANCE IF HAS A DESTINATION
            
                CalculateBestAction();

                yield return null;
            }
        }
        
        public DieComponent GetDieComponent()
        {
            return _dieComponent;
        }

        protected override void UpdateVisibleRivals()
        {
            _visibleRivals = CombatManager.Instance.GetVisibleRivals
                <AIEnemy, AIEnemyContext, AttackComponent, AllyDamageComponent, 
                    AIAllyContext, AllyAttackComponent, DamageComponent>(this);

            _context.SetIsSeeingARival(_visibleRivals.Count != 0);

            _enemiesThatTargetsMe = CombatManager.Instance.FilterEnemiesThatTargetsMe(GetCombatAgentInstance(), _visibleRivals);
        }

        private void UpdateDistancesToEnemiesThatTargetsMe()
        {
            _context.SetDistancesToEnemiesThatThreatMe(
                CombatManager.Instance.GetDistancesToGivenEnemies(transform.position, _enemiesThatTargetsMe));
        }

        protected override void CalculateBestAction()
        {
            CombatManager.Instance.CalculateBestAction(this);
        }

        public override void OnAttackAvailableAgain(AllyAttackComponent attackComponent)
        {
            base.OnAttackAvailableAgain(attackComponent);

            _context.SetCanDefeatEnemy(CalculateIfGivenAttackCanDefeatEnemy(attackComponent));
            _context.SetCanStunEnemy(CalculateIfGivenAttackCanStunEnemy(attackComponent));
        }

        private void CalculateIfCanDefeatEnemy()
        {
            bool canDefeat;
            
            foreach (AllyAttackComponent allyAttackComponent in _attackComponents)
            {
                if (allyAttackComponent.IsOnCooldown())
                {
                    continue;
                }

                canDefeat = CalculateIfGivenAttackCanDefeatEnemy(allyAttackComponent);

                if (!canDefeat)
                {
                    continue;
                }
                
                _context.SetCanDefeatEnemy(true);
                return;
            }
            
            _context.SetCanDefeatEnemy(false);
        }

        private bool CalculateIfGivenAttackCanDefeatEnemy(AllyAttackComponent allyAttackComponent)
        {
            return allyAttackComponent.GetDamage() >= _context.GetRivalHealth();
        }

        private void CalculateIfCanStunEnemy()
        {
            bool canStun;
            
            foreach (AllyAttackComponent allyAttackComponent in _attackComponents)
            {
                if (allyAttackComponent.IsOnCooldown())
                {
                    continue;
                }

                canStun = CalculateIfGivenAttackCanStunEnemy(allyAttackComponent);

                if (!canStun)
                {
                    continue;
                }
                
                _context.SetCanStunEnemy(true);
                return;
            }
            
            _context.SetCanStunEnemy(false);
        }

        private bool CalculateIfGivenAttackCanStunEnemy(AllyAttackComponent allyAttackComponent)
        {
            return !(allyAttackComponent.GetStressDamage() + _context.GetRivalCurrentStress() < 
                     _context.GetRivalMaximumStress());
        }

        public void Attack()
        {
            AllyAttackComponent attackComponent = ReturnNextAttack();
            
            Attacking();

            StartCastingAnAttack(attackComponent);
        }

        private void StartCastingAnAttack(AllyAttackComponent allyAttackComponent)
        {
            if (allyAttackComponent.IsOnCooldown())
            {
                GetContext().SetIsAttacking(false);
                return;
            }
            
            AIAttackCollider attackCollider = _attacksColliders[allyAttackComponent];
            
            attackCollider.SetParent(transform);
            StartCoroutine(StartAttackCastTimeCoroutine(allyAttackComponent, attackCollider));
        }

        private void PutAttackOnCooldown(AllyAttackComponent attackComponent)
        {
            NotAttacking();
            StartCoroutine(StartCooldownCoroutine(attackComponent));
        }

        private IEnumerator StartAttackCastTimeCoroutine(AllyAttackComponent allyAttackComponent, 
            AIAttackCollider attackCollider)
        {
            allyAttackComponent.StartCastTime();
            
            attackCollider.gameObject.SetActive(true);
            
            while (allyAttackComponent.IsCasting())
            {
                allyAttackComponent.DecreaseCurrentCastTime();
                yield return null;
            }
            
            attackCollider.gameObject.SetActive(true);

            yield return null;
            
            attackCollider.StartInflictingDamage();

            if (allyAttackComponent.DoesDamageOverTime())
            {
                StartCoroutine(StartDamageOverTime(allyAttackComponent, attackCollider));
                yield break;
            }
            
            RotateToNextPathCorner();
            PutAttackOnCooldown(allyAttackComponent);
            attackCollider.Deactivate();
        }

        private IEnumerator StartDamageOverTime(AllyAttackComponent allyAttackComponent, 
            AIAttackCollider attackCollider)
        {
            while (allyAttackComponent.DidDamageOverTimeFinished())
            {
                allyAttackComponent.DecreaseRemainingTimeDealingDamage();
                yield return null;
            }
           
            RotateToNextPathCorner();
            PutAttackOnCooldown(allyAttackComponent);
            attackCollider.Deactivate();
        }

        private IEnumerator StartCooldownCoroutine(AllyAttackComponent allyAttackComponent)
        {
            allyAttackComponent.StartCooldown();
            while (allyAttackComponent.IsOnCooldown())
            {
                allyAttackComponent.DecreaseCooldown();
                yield return null;
            }
            
            OnAttackAvailableAgain(allyAttackComponent);
        }

        public void RequestHelp()
        {
            
        }

        public void DodgeAttack(VectorComponent positionToDodge)
        {
            ContinueNavigation();

            NavMeshAgentComponent navMeshAgentComponent = GetNavMeshAgentComponent();

            navMeshAgentComponent.GetNavMeshAgent().stoppingDistance = 1;
            
            _context.SetStoppingDistance(1);
            
            ECSNavigationManager.Instance.UpdateNavMeshAgentVectorDestination(GetNavMeshAgentComponent(),positionToDodge);
        }

        public void WarnOncomingDamage(AttackComponent attackComponent, AIEnemyAttackCollider enemyAttackCollider)
        {
            _oncomingEnemyAttacks.Add(enemyAttackCollider);
            
            _context.SetIsUnderAttack(true);
            _context.SetOncomingAttackDamage(_context.GetOncomingAttackDamage() + attackComponent.GetDamage());
        }
        
        
        public void FreeOfWarnArea(AttackComponent attackComponent, AIEnemyAttackCollider enemyAttackCollider)
        {
            _oncomingEnemyAttacks.Remove(enemyAttackCollider);
            
            _context.SetOncomingAttackDamage(_context.GetOncomingAttackDamage() - attackComponent.GetDamage());
            CheckIfOutOfDanger();
        }

        private void CheckIfOutOfDanger()
        {
            _context.SetIsUnderAttack(_oncomingEnemyAttacks.Count != 0);

            if (_oncomingEnemyAttacks.Count != 0)
            {
                return;
            }

            NavMeshAgentComponent navMeshAgentComponent = GetNavMeshAgentComponent();

            navMeshAgentComponent.GetNavMeshAgent().stoppingDistance = 7;
            
            _context.SetStoppingDistance(7);

            if (_lastDestination != null)
            {
                ECSNavigationManager.Instance.UpdateNavMeshAgentVectorDestination(GetNavMeshAgentComponent(), _lastDestination);
                return;
            }
            
            ECSNavigationManager.Instance.UpdateNavMeshAgentTransformDestination(GetNavMeshAgentComponent(), 
                new TransformComponent(GetContext().GetRivalTransform()));
        }

        public override void OnReceiveDamage(DamageComponent damageComponent)
        {
            _context.SetHealth(_context.GetHealth() - damageComponent.GetDamage());

            if (_context.GetHealth() != 0)
            {
                //TODO FEEDBACK
                return;
            }
            
            OnDefeated();
        }

        protected override void OnDefeated()
        {
            CombatManager.Instance.OnAllyDefeated(this);
        }

        private void OnDie()
        {
            ECSNavigationManager.Instance.RemoveNavMeshAgentEntity(GetNavMeshAgentComponent());
            Destroy(gameObject);
        }

        private void OnBeingRescued()
        {
            
        }

        public override AIAgentType GetAIAgentType()
        {
            return _aiAllySpecs.aiAgentType;
        }

        public override AIAllyContext GetContext()
        {
            return _context;
        }

        public void SetOncomingAttackDamage(uint oncomingAttackDamage)
        {
            _context.SetOncomingAttackDamage(oncomingAttackDamage);
        }

        public void SetEnemyHealth(uint enemyHealth)
        {
            _context.SetRivalHealth(enemyHealth);
            CalculateIfCanDefeatEnemy();
        }

        public void SetEnemyMaximumStress(float enemyMaximumStress)
        {
            _context.SetRivalMaximumStress(enemyMaximumStress);
        }

        public void SetEnemyCurrentStress(float enemyCurrentStress)
        {
            _context.SetRivalCurrentStress(enemyCurrentStress);
            CalculateIfCanStunEnemy();
        }

        public void SetIsEnemyStunned(bool isEnemyStunned)
        {
            _context.SetIsEnemyStunned(isEnemyStunned);
        }

        public void SetIsUnderThreat(bool isUnderThreat)
        {
            _context.SetIsUnderThreat(isUnderThreat);
        }

        public void SetIsUnderAttack(bool isUnderAttack)
        {
            _context.SetIsUnderAttack(isUnderAttack);
        }

        public void SetState(AIAllyOrders allyOrder)
        {
            _context.SetIsInRetreatState(allyOrder == AIAllyOrders.RETREAT);
            _context.SetIsInAttackState(allyOrder == AIAllyOrders.ATTACK);
            _context.SetIsInFleeState(allyOrder == AIAllyOrders.FLEE);
        }

        public List<AllyAttackComponent> GetAllyAttackComponents()
        {
            return _attackComponents;
        }

        public bool IsAI()
        {
            return _isAI;
        }
    }
}