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
using ECS.Components.AI.Navigation;
using Managers;
using UnityEngine;
using Utilities;

namespace ECS.Entities.AI.Combat
{
    public class AIAlly : AICombatAgentEntity<AIAllyContext, AllyAttackComponent, DamageComponent, AIAllyAction>
    {
        [SerializeField] private AIAllySpecs _aiAllySpecs;

        private AIAllyUtilityFunction _aiAllyUtilityFunction = new AIAllyUtilityFunction();

        private List<uint> _enemiesThatTargetsMe = new List<uint>();

        private List<AttackColliderCountdown> _oncomingEnemyAttacks = new List<AttackColliderCountdown>();

        private AIAlly _playerBody;
        private Transform _playerTransform;

        private Action _releaseAgentSlot;
        private Func<AgentSlotPosition> _requestAgentSlotPositionFunc;

        [SerializeField] private bool _isAI;

        private void Start()
        {
            _utilityFunction = new AIAllyUtilityFunction();
            InitiateDictionaries();
            Setup();
            InstantiateAttackComponents(_aiAllySpecs.aiAttacks);
            CalculateMinimumAndMaximumRangeToAttacks(_attackComponents);

            _raysTargetsLayerMask = ~(int)(Math.Pow(2, GameManager.Instance.GetAllyAttackZoneLayer()) + 
                                                       Math.Pow(2, GameManager.Instance.GetGroundLayer()));

            CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();

            float radius = capsuleCollider.radius;

            _surroundingSlots = new SurroundingSlots(radius + _aiAllySpecs.rivalsPositionRadius);
            
            _context = new AIAllyContext(_aiAllySpecs.totalHealth, radius, capsuleCollider.height,
                _aiAllySpecs.sightMaximumDistance, _minimumRangeToCastAnAttack, _maximumRangeToCastAnAttack, transform, 
                _aiAllySpecs.minimumEnemiesInsideAlertRadiusToFlee, 
                _aiAllySpecs.alertRadius, _aiAllySpecs.safetyRadius);
            
            CombatManager.Instance.AddAIAlly(this);
            ECSNavigationManager.Instance.AddNavMeshAgentEntity(GetAgentID(), GetNavMeshAgentComponent(), radius);
            
            InstantiateAttacksColliders();

            StartCoroutine(UpdateCountdowns());

            if (!_isAI)
            {
                StartCoroutine(BrainCellWaitSubscribes());
                ECSNavigationManager.Instance.RemoveNavMeshAgentEntity(GetAgentID(), false);
                return;
            }

            EventsManager.OnSwitch += UpdatePlayerBody;

            StartCoroutine(CommonersWaitSubscribes());
        }

        private IEnumerator BrainCellWaitSubscribes()
        {
            while (EventsManager.OnSwitch == null ||
                   EventsManager.OnSwitch.GetInvocationList().Length < 2)
            {
                yield return null;
            }

            EventsManager.OnSwitch(this);
        }

        private IEnumerator CommonersWaitSubscribes()
        {
            while (EventsManager.OnSwitch == null ||
                   EventsManager.OnSwitch.GetInvocationList().Length < 2)
            {
                yield return null;
            }

            float time = 0;

            while (time < 0.2f)
            {
                time += Time.deltaTime;
                yield return null;
            }
            
            base.StartUpdate();
        }

        protected override void InitiateDictionaries()
        {
            _actions = new Dictionary<AIAllyAction, Action>
            {
                { AIAllyAction.FOLLOW_PLAYER , FollowPlayer },
                { AIAllyAction.CHOOSE_NEW_RIVAL , RequestRival },
                { AIAllyAction.GET_CLOSER_TO_RIVAL , GetCloserToRival },
                { AIAllyAction.ROTATE , Rotate },
                { AIAllyAction.ATTACK , Attack },
                { AIAllyAction.FLEE , Flee },
                { AIAllyAction.DODGE_ATTACK , Dodge }
            };
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
                GameObject colliderObject = null;

                switch (attackComponent.GetAIAttackAoEType())
                {
                    case AIAttackAoEType.RECTANGLE_AREA:
                        colliderObject = Instantiate(_rectangleAttackColliderPrefab);
                        AIAllyRectangleAttackCollider rectangleAttackCollider =
                            colliderObject.GetComponent<AIAllyRectangleAttackCollider>();
                        
                        rectangleAttackCollider.SetOwner(GetAgentID(), _context);
                        rectangleAttackCollider.SetRectangleAttackComponent((AllyRectangleAttackComponent)attackComponent);
                        rectangleAttackCollider.SetAttackTargets((int)Mathf.Pow(2, layerTarget));
                        _attacksColliders.Add(attackComponent, rectangleAttackCollider);
                        break;

                    case AIAttackAoEType.CIRCLE_AREA:
                        colliderObject = Instantiate(_circleAttackColliderPrefab);
                        AIAllyCircleAttackCollider circleAttackCollider = 
                            colliderObject.GetComponent<AIAllyCircleAttackCollider>();
                        
                        circleAttackCollider.SetOwner(GetAgentID(), _context);
                        circleAttackCollider.SetCircleAttackComponent((AllyCircleAttackComponent)attackComponent);
                        circleAttackCollider.SetAttackTargets((int)Mathf.Pow(2, layerTarget));
                        _attacksColliders.Add(attackComponent, circleAttackCollider);
                        break;

                    case AIAttackAoEType.CONE_AREA:
                        AIAllyConeAttackCollider coneAttackCollider = colliderObject.GetComponent<AIAllyConeAttackCollider>();
                        
                        coneAttackCollider.SetOwner(GetAgentID(), _context);
                        coneAttackCollider.SetConeAttackComponent((AllyConeAttackComponent)attackComponent);
                        coneAttackCollider.SetAttackTargets((int)Mathf.Pow(2, layerTarget));
                        _attacksColliders.Add(attackComponent, coneAttackCollider);
                        break;
                }

                colliderObject.SetActive(false);
            }
        }

        #region AI Loop

        private void UpdatePlayerBody(AIAlly playerBody)
        {
            _playerBody = playerBody;
            _playerTransform = _playerBody.GetNavMeshAgentComponent().GetTransformComponent().GetTransform();
            _context.SetDoesBrainCellSwitched(true);
        }

        public void CallStartUpdate()
        {
            EventsManager.OnSwitch += UpdatePlayerBody;
            StartUpdate();
        }

        protected override void StartUpdate()
        {
            StartCoroutine(EnsurePlayerTransformIsNotNull());
        }

        private IEnumerator EnsurePlayerTransformIsNotNull()
        {
            while (_playerTransform == null)
            {
                yield return null;
            }
            
            _isAI = true;
            _navMeshAgent.enabled = true;
            ECSNavigationManager.Instance.ReturnNavMeshAgentEntity(GetAgentID(), GetNavMeshAgentComponent());
            base.StartUpdate();
            
        }

        public void CallStopUpdate()
        {
            EventsManager.OnSwitch -= UpdatePlayerBody;
            EventsManager.OnSwitch(this);
            StopUpdate();
        }

        protected override void StopUpdate()
        {
            _navMeshAgent.enabled = false;
            ECSNavigationManager.Instance.RemoveNavMeshAgentEntity(GetAgentID(), false);
            _playerBody = null;
            _playerTransform = null;
            _isAI = false;
            base.StopUpdate();
        }

        protected override IEnumerator UpdateCoroutine()
        {
            while (_isAI)
            {
                UpdateVisibleRivals();

                UpdateVectorToRival();

                UpdateVectorsToEnemiesThatTargetsMe();

                if (_context.IsAttacking())
                {
                    yield return null;
                    continue;
                }
                
                //LaunchRaycasts();

                if (_isRotating)
                {
                    yield return null;
                    continue;
                }
            
                CalculateBestAction();

                if (!_context.HasATarget() && !_context.IsFollowingAlly())
                {
                    yield return null;
                    continue;
                }

                AgentSlotPosition agentSlotPosition = _requestAgentSlotPositionFunc();

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
            _visibleRivals = CombatManager.Instance.GetVisibleRivals<AIEnemy, AIEnemyContext, AttackComponent, 
                AllyDamageComponent, AIEnemyAction, AIAllyContext, AllyAttackComponent, DamageComponent, AIAllyAction>(this);

            SetIsSeeingARival(_visibleRivals.Count != 0);

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

        #region FSM

        private void FollowPlayer()
        {
            ShowActionDebugLogs(name + " Following Player");

            if (_releaseAgentSlot != null)
            {
                _releaseAgentSlot();    
            }

            _releaseAgentSlot = () => _playerBody.ReleaseAgentSlot(GetAgentID());
            
            _requestAgentSlotPositionFunc = () => 
                _playerBody.GetAgentSlotPosition(_playerTransform.position - transform.position, _context.GetRadius());
            
            SetHasATarget(false);
            _context.SetIsFollowingAlly(true);
            _context.SetDoesBrainCellSwitched(false);
            SetDestination(new TransformComponent(_playerTransform));
        }

        protected override void RequestRival()
        {
            ShowActionDebugLogs(name + " Requesting Rival");

            if (_visibleRivals.Count == 0)
            {
                return;
            }

            List<uint> reachableRivals = CombatManager.Instance
                .GetReachableRivals<AIEnemy, AIEnemyContext, AttackComponent, AllyDamageComponent, AIEnemyAction>(
                    GetNavMeshAgentComponent().GetNavMeshAgent(), _visibleRivals, AIAgentType.ENEMY);

            if (reachableRivals.Count == 0)
            {
                return;
            }

            List<uint> possibleRivals = CombatManager.Instance.GetPossibleRivals(reachableRivals);

            if (possibleRivals.Count == 0)
            {
                return;
            }

            AIEnemy previousEnemy = CombatManager.Instance.RequestEnemy(_context.GetRivalID());

            if (previousEnemy != null)
            {
                previousEnemy.ReleaseAgentSlot(GetAgentID());
            }

            uint targetID = ObtainTargetID(possibleRivals);
            
            OnTargetAcquired(targetID, CombatManager.Instance.RequestEnemy(targetID).GetContext());
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
                targetID = CombatManager.Instance.GetClosestRivalID<AIEnemy, AIEnemyContext, AttackComponent,
                    AllyDamageComponent, AIEnemyAction>(GetNavMeshAgentComponent().GetTransformComponent(),
                    possibleRivals, AIAgentType.ENEMY);
            }

            return targetID;
        }

        private void OnTargetAcquired(uint enemyID, AIEnemyContext enemyContext)
        {
            SetRivalIndex(enemyID);
            SetRivalRadius(enemyContext.GetRadius());
            SetRivalHeight(enemyContext.GetHeight());
            SetHasATarget(true);
            SetEnemyHealth(enemyContext.GetHealth());
            SetEnemyMaximumStress(enemyContext.GetMaximumStress());
            SetEnemyCurrentStress(enemyContext.GetCurrentStress());
            SetRivalTransform(enemyContext.GetAgentTransform());

            _context.SetIsFollowingAlly(false);

            if (_releaseAgentSlot != null)
            {
                _releaseAgentSlot();    
            }

            _releaseAgentSlot = () =>
            {
                AIEnemy enemy = CombatManager.Instance.RequestEnemy(_context.GetRivalID());

                if (enemy == null)
                {
                    return;
                }

                enemy.ReleaseAgentSlot(GetAgentID());
            };


            _requestAgentSlotPositionFunc = () => CombatManager.Instance.RequestEnemy(_context.GetRivalID())
                .GetAgentSlotPosition(_context.GetVectorToRival(), _context.GetRadius());
        }

        private void GetCloserToRival()
        {
            ShowActionDebugLogs(name + " Getting Closer To Rival");
            
            SetDestination(CombatManager.Instance.RequestEnemy(_context.GetRivalID())
                .GetNavMeshAgentComponent().GetTransformComponent());
        }

        private void Attack()
        {
            ShowActionDebugLogs(name + " Attacking");
            
            AllyAttackComponent attackComponent = ReturnNextAttack();
            
            Attacking();

            StartCastingAttack(attackComponent);
        }

        private void Flee()
        {
            ShowActionDebugLogs(name + " Fleeing");
            
            ContinueNavigation();

            List<float> angles = new List<float>();

            foreach (Vector3 vector in _context.GetVectorsToEnemiesThatTargetsMe())
            {
                angles.Add(MathUtil.VectorXZToYAxisAngle(vector));   
            }
            
            angles.Sort();

            (int, float) widerSubtendedAngle = GetWiderSubtendedAngle(angles);

            Vector3 position = MathUtil.YAxisAngleToVectorXZ((angles[widerSubtendedAngle.Item1] + widerSubtendedAngle.Item2 / 2) % 360);

            position *= _context.GetSafetyRadius() * 2;
            
            ECSNavigationManager.Instance.UpdateNavMeshAgentDestination(GetAgentID(), 
                new VectorComponent(transform.position + position));
        }

        private (int, float) GetWiderSubtendedAngle(List<float> angles)
        {
            float longestDistanceToNextAngle = 0;

            int chosenAngleIndex = 0;
            
            float currentAngle;
            float nextAngle;
            float currentDistanceToNextAngle;

            int anglesCount = angles.Count;

            for (int i = 0; i < anglesCount; i++)
            {
                currentAngle = angles[i];
                nextAngle = angles[(i + 1) % anglesCount];

                if (currentAngle > nextAngle)
                {
                    currentDistanceToNextAngle = Math.Abs(nextAngle + (360 - currentAngle));
                }
                else
                {
                    currentDistanceToNextAngle = nextAngle - currentAngle;
                }

                if (longestDistanceToNextAngle > currentDistanceToNextAngle)
                {
                    continue;
                }

                longestDistanceToNextAngle = currentDistanceToNextAngle;
                chosenAngleIndex = i;
            }

            return (chosenAngleIndex, longestDistanceToNextAngle);
        }

        private void Dodge()
        {
            return;
            ShowActionDebugLogs(name + " Dodging");
            
            ContinueNavigation();

            //TODO
            VectorComponent positionToDodge = new VectorComponent(new Vector3());

            _numberOfVicinityRays *= 2;

            _raysOpeningAngle = 360f;
            
            ECSNavigationManager.Instance.UpdateNavMeshAgentDestination(GetAgentID(), positionToDodge);
        }

        #endregion

        #region Attacks Managing

        #region Own Attacks

        protected override void OnAttackAvailableAgain(AllyAttackComponent attackComponent)
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

        protected override void StartCastingAttack(AllyAttackComponent allyAttackComponent)
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

        protected override IEnumerator StartAttackCastTimeCoroutine(AllyAttackComponent allyAttackComponent, 
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
            
            PutAttackOnCooldown(allyAttackComponent);
            attackCollider.Deactivate();

            if (!_isAI)
            {
                yield break;
            }
            
            Rotate(); 
        }

        protected override IEnumerator StartDamageOverTime(AllyAttackComponent allyAttackComponent, 
            AIAttackCollider attackCollider)
        {
            while (allyAttackComponent.DidDamageOverTimeFinished())
            {
                allyAttackComponent.DecreaseRemainingTimeDealingDamage();
                yield return null;
            }
            
            PutAttackOnCooldown(allyAttackComponent);
            attackCollider.Deactivate();

            if (!_isAI)
            {
                yield break;
            }
            
            Rotate();    
        }

        protected override void PutAttackOnCooldown(AllyAttackComponent attackComponent)
        {
            NotAttacking();
            StartCoroutine(StartCooldownCoroutine(attackComponent));
        }

        protected override IEnumerator StartCooldownCoroutine(AllyAttackComponent allyAttackComponent)
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

        #region Rival Attacks

        public void WarnOncomingDamage(AttackComponent attackComponent, AIEnemyAttackCollider enemyAttackCollider, 
            float timeRemaining)
        {
            _oncomingEnemyAttacks.Add(new AttackColliderCountdown
            {
                aiEnemyAttackCollider = enemyAttackCollider,
                timeRemaining = timeRemaining
            });
            
            _context.SetIsUnderAttack(true);
            _context.SetOncomingAttackDamage(_context.GetOncomingAttackDamage() + attackComponent.GetDamage());
        }
        
        public void FreeOfWarnArea(AttackComponent attackComponent, AIEnemyAttackCollider enemyAttackCollider)
        {
            for (int i = 0; i < _oncomingEnemyAttacks.Count; i++)
            {
                if (_oncomingEnemyAttacks[i].aiEnemyAttackCollider != enemyAttackCollider)
                {
                    return;
                }   
                
                _oncomingEnemyAttacks.RemoveAt(i);
                
                break;
            }
            
            _context.SetOncomingAttackDamage(_context.GetOncomingAttackDamage() - attackComponent.GetDamage());
            CheckIfOutOfDanger();
        }

        private void CheckIfOutOfDanger()
        {
            bool isInDanger = _oncomingEnemyAttacks.Count != 0;
            
            _context.SetIsUnderAttack(isInDanger);

            if (isInDanger)
            {
                return;
            }

            if (!_isAI)
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
            SetHealth(_context.GetHealth() - damageComponent.GetDamage());

            if (_context.GetHealth() != 0)
            {
                //TODO FEEDBACK
                return;
            }
            
            OnDefeated();
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
            
            EventsManager.OnSwitch -= UpdatePlayerBody;
            CombatManager.Instance.OnAllyDefeated(this);
            ECSNavigationManager.Instance.RemoveNavMeshAgentEntity(GetAgentID(), true);
        }

        #endregion

        #endregion

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

        private void SetEnemyMaximumStress(float enemyMaximumStress)
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

        private IEnumerator UpdateCountdowns()
        {
            float lowestCooldown;
            
            while (true)
            {
                lowestCooldown = Mathf.Infinity;
                
                foreach (AttackColliderCountdown attackColliderCountdown in _oncomingEnemyAttacks)
                {
                    attackColliderCountdown.timeRemaining -= Time.deltaTime;

                    if (attackColliderCountdown.timeRemaining > lowestCooldown)
                    {
                        continue;
                    }
                    
                    _context.SetTimeToNextEnemyMeleeAttack(attackColliderCountdown.timeRemaining);
                }
                
                yield return null;
            }
        }
    }
}