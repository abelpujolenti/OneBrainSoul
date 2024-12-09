using System;
using System.Collections;
using System.Collections.Generic;
using AI;
using AI.Combat;
using AI.Combat.Ally;
using AI.Combat.AttackColliders;
using AI.Combat.Position;
using AI.Combat.ScriptableObjects;
using ECS.Components.AI.Combat;
using ECS.Components.AI.Navigation;
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

        [SerializeField] private bool _isAI;

        private void Start()
        {
            Setup();
            InstantiateAttackComponents(_aiAllySpecs.aiAttacks);
            CalculateMinimumAndMaximumRangeToAttacks(_attackComponents);

            _raysTargetsLayerMask = ~(int)(Math.Pow(2, GameManager.Instance.GetAllyAttackZoneLayer()) + 
                                                       Math.Pow(2, GameManager.Instance.GetGroundLayer()));

            CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();

            float radius = capsuleCollider.radius;

            _surroundingSlots = new SurroundingSlots(radius + _aiAllySpecs.rivalsPositionRadius);
            
            _context = new AIAllyContext(_aiAllySpecs.totalHealth, radius,
                _aiAllySpecs.sightMaximumDistance, _minimumRangeToCastAnAttack, _maximumRangeToCastAnAttack, transform, 
                _aiAllySpecs.minimumEnemiesInsideAlertRadiusToFlee, capsuleCollider.height, 
                _aiAllySpecs.alertRadius, _aiAllySpecs.safetyRadius);
            
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
                        _attackComponents.Add(new AllyRectangleAttackComponent(GetAgentID(), aiAllyAttack));
                        break;
                    
                    case AIAttackAoEType.CIRCLE_AREA:
                        _attackComponents.Add(new AllyCircleAttackComponent(GetAgentID(), aiAllyAttack));
                        break;
                    
                    case AIAttackAoEType.CONE_AREA:
                        _attackComponents.Add(new AllyConeAttackComponent(GetAgentID(), aiAllyAttack));
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

        #region AI Loop

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

                UpdateVectorsToEnemiesThatTargetsMe();

                if (_context.IsAttacking())
                {
                    yield return null;
                    continue;
                }
                
                LaunchRaycasts();

                if (_isRotating)
                {
                    yield return null;
                    continue;
                }
            
                CalculateBestAction();

                if (!_context.HasATarget())
                {
                    yield return null;
                    continue;
                }
                
                RivalSlotPosition rivalSlotPosition = CombatManager.Instance.RequestEnemy(_context.GetRivalID())
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
            _visibleRivals = CombatManager.Instance.GetVisibleRivals
                <AIEnemy, AIEnemyContext, AttackComponent, AllyDamageComponent, 
                    AIAllyContext, AllyAttackComponent, DamageComponent>(this);

            _context.SetIsSeeingARival(_visibleRivals.Count != 0);

            _enemiesThatTargetsMe = CombatManager.Instance.FilterEnemiesThatTargetsMe(GetAgentID(), _visibleRivals);
        }

        private void UpdateVectorsToEnemiesThatTargetsMe()
        {
            (List<Vector3>, List<float>) vectorsAndDistancesToEnemies =
                CombatManager.Instance.GetVectorsAndDistancesToGivenEnemies(transform.position, _enemiesThatTargetsMe);
            
            _context.SetVectorsToEnemiesThatTargetsMe(vectorsAndDistancesToEnemies.Item1);
            _context.SetDistancesToEnemiesThatTargetsMe(vectorsAndDistancesToEnemies.Item2);
        }

        #endregion

        #region UBS

        

        #endregion

        #region FSM

        protected override void CalculateBestAction()
        {
            CombatManager.Instance.CalculateBestAction(this);
        }

        public void Attack()
        {
            AllyAttackComponent attackComponent = ReturnNextAttack();
            
            Attacking();

            StartCastingAnAttack(attackComponent);
        }

        public void DodgeAttack(VectorComponent positionToDodge)
        {
            ContinueNavigation();

            _numberOfVicinityRays *= 2;

            _raysOpeningAngle = 360f;
            
            ECSNavigationManager.Instance.UpdateNavMeshAgentDestination(GetAgentID(), positionToDodge);
        }

        public void Flee()
        {
            //Debug.Log("Fleeing");
            
            ContinueNavigation();

            List<float> angles = new List<float>();

            foreach (Vector3 vector in _context.GetVectorsToEnemiesThatTargetsMe())
            {
                angles.Add(MathUtil.VectorToAngle(-vector));   
            }
            
            angles.Sort();

            float shortestDistanceToNextAngle = Mathf.Infinity;

            int chosenAngleIndex = 0;
            
            float currentAngle;
            float nextAngle;
            float currentDistanceToNextAngle;

            int anglesCount = angles.Count;

            for (int i = 0; i < anglesCount; i++)
            {
                currentAngle = angles[i];
                nextAngle = angles[(i + 1) % (anglesCount - 1)];

                if (currentAngle > nextAngle)
                {
                    currentDistanceToNextAngle = Math.Abs(currentAngle + (360 - nextAngle));
                }
                else
                {
                    currentDistanceToNextAngle = nextAngle - currentAngle;
                }

                if (shortestDistanceToNextAngle > currentDistanceToNextAngle)
                {
                    continue;
                }

                chosenAngleIndex = i;
            }

            if (chosenAngleIndex == anglesCount - 1)
            {
                shortestDistanceToNextAngle = Math.Abs(angles[chosenAngleIndex] + (360 - angles[0]));
            }
            else
            {
                shortestDistanceToNextAngle = angles[chosenAngleIndex + 1] - angles[chosenAngleIndex];
            }

            Vector3 position = MathUtil.AngleToVector(angles[chosenAngleIndex] + shortestDistanceToNextAngle / 2);

            position *= _context.GetSafetyRadius() * 2;
            
            ECSNavigationManager.Instance.UpdateNavMeshAgentDestination(GetAgentID(), new VectorComponent(position));
        }

        #endregion

        #region Attacks Managing

        #region Own Attacks

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

        #endregion

        #region Enemy Attacks

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

            if (_lastDestination != null)
            {
                ECSNavigationManager.Instance.UpdateNavMeshAgentDestination(GetAgentID(), _lastDestination);
                return;
            }
            
            ECSNavigationManager.Instance.UpdateNavMeshAgentDestination(GetAgentID(),
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

        #endregion

        #endregion

        protected override void OnDefeated()
        {
            CombatManager.Instance.OnAllyDefeated(this);
        }

        private void OnBeingRescued()
        {
            
        }

        public override AIAgentType GetAIAgentType()
        {
            return _aiAllySpecs.aiAgentType;
        }

        #region Context

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

        #endregion

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