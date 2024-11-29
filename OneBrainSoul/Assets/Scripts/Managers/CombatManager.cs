using System;
using System.Collections;
using System.Collections.Generic;
using AI;
using AI.Combat;
using AI.Combat.Ally;
using AI.Combat.Enemy;
using AI.Combat.ScriptableObjects;
using ECS.Components.AI.Combat;
using ECS.Components.AI.Navigation;
using ECS.Entities.AI.Combat;
using Interfaces.AI.Navigation;
using Interfaces.AI.UBS.BaseInterfaces.Get;
using Unity.AI.Navigation;
using UnityEngine;

namespace Managers
{
    public class CombatManager : MonoBehaviour
    {
        private static CombatManager _instance;

        public static CombatManager Instance => _instance;

        [SerializeField] private GameObject _enemyRectangleAttackColliderPrefab;
        [SerializeField] private GameObject _enemyCircleAttackColliderPrefab;

        private readonly Dictionary<AIAgentType, Delegate> _returnTheSameAgentsType = new Dictionary<AIAgentType, Delegate>
        {
            { AIAgentType.ALLY, new Func<List<AIAlly>>(() => 
                _instance.ReturnAllDictionaryValuesInAList<AIAlly ,AIAllyContext, AllyAttackComponent, DamageComponent>(
                    _instance._aiAllies)) },
            
            { AIAgentType.ENEMY, new Func<List<AIEnemy>>(() => 
                _instance.ReturnAllDictionaryValuesInAList<AIEnemy, AIEnemyContext, AttackComponent, AllyDamageComponent>(
                    _instance._aiEnemies)) }
        };
        
        private Dictionary<uint, AIAlly> _aiAllies = new Dictionary<uint, AIAlly>();
        private Dictionary<uint, AIEnemy> _aiEnemies = new Dictionary<uint, AIEnemy>();

        private readonly Dictionary<AIAgentType, int> _targetsLayerMask = new Dictionary<AIAgentType, int>
        {
            { AIAgentType.ALLY, (int)(Math.Pow(2, 7) + Math.Pow(2, 6)) },
            { AIAgentType.ENEMY, (int)(Math.Pow(2, 8) + Math.Pow(2, 6)) }
        };

        private readonly Dictionary<AIAllyAction, Action<AIAlly>> _aiAllyActions = new Dictionary<AIAllyAction, Action<AIAlly>>
        {
            { AIAllyAction.FOLLOW_PLAYER , ally => Instance.AllyFollowPlayer(ally)},
            { AIAllyAction.CHOOSE_NEW_RIVAL , ally => Instance.AllyRequestRival(ally) },
            { AIAllyAction.GET_CLOSER_TO_RIVAL , ally => Instance.AllyGetCloserToEnemy(ally)},
            { AIAllyAction.ROTATE , ally => ally.Rotate()},
            { AIAllyAction.ATTACK , ally => Instance.AllyAttack(ally)},
            { AIAllyAction.FLEE , ally => Instance.AllyFlee(ally)},
            { AIAllyAction.DODGE_ATTACK , ally => Instance.AllyDodge(ally)}
        };
        
        private readonly Dictionary<AIEnemyAction, Action<AIEnemy>> _aiEnemyActions = new Dictionary<AIEnemyAction, Action<AIEnemy>>
        {
            { AIEnemyAction.PATROL , enemy => Instance.EnemyPatrol(enemy)},
            { AIEnemyAction.CHOOSE_NEW_RIVAL , enemy => Instance.EnemyRequestRival(enemy)},
            { AIEnemyAction.GET_CLOSER_TO_RIVAL , enemy => Instance.EnemyGetCloserToAlly(enemy)},
            { AIEnemyAction.ROTATE , enemy => enemy.Rotate()},
            { AIEnemyAction.ATTACK , enemy => Instance.EnemyAttack(enemy)},
            { AIEnemyAction.FLEE , enemy => Instance.EnemyFlee(enemy)}
        };

        private Dictionary<AttackComponent, AIEnemyAttackCollider> _enemiesAttacksColliders =
            new Dictionary<AttackComponent, AIEnemyAttackCollider>();

        private AIAllyUtilityFunction _allyUtilityFunction = new AIAllyUtilityFunction();
        private AIEnemyUtilityFunction _enemyUtilityFunction = new AIEnemyUtilityFunction();
        
        //ERASE!!!
        [SerializeField] private bool _showActionsDebugLogs;
        [SerializeField] private List<GameObject> FLEE_POINTS;
        private List<Vector3> TERRAIN_POSITIONS;
        private Dictionary<AIAlly, int> FLEE_POINTS_RECORD = new Dictionary<AIAlly, int>(); 

        private void ShowActionDebugLogs(string message)
        {
            if (!_showActionsDebugLogs)
            {
                return;
            }
            
            Debug.Log(message);
        }
        //

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;

                DontDestroyOnLoad(gameObject);
                
                //ERASE!!!
                TERRAIN_POSITIONS = GetTerrainPositions(FLEE_POINTS);
                StartCoroutine(UpdateFleeMovement());
                //

                return;
            }

            Destroy(gameObject);
        }

        #region UBS
        
        public List<uint> GetVisibleRivals<TAgent, TRivalContext, TRivalAttackComponent, TRivalDamageComponent, 
            TOwnContext, TOwnAttackComponent, TOwnDamageComponent>(
            AICombatAgentEntity<TOwnContext, TOwnAttackComponent, TOwnDamageComponent> aiCombatAgent)
        
            where TAgent : AICombatAgentEntity<TRivalContext, TRivalAttackComponent, TRivalDamageComponent>
            where TRivalContext : AICombatAgentContext
            where TRivalAttackComponent : AttackComponent
            where TRivalDamageComponent : DamageComponent
            where TOwnContext : AICombatAgentContext
            where TOwnAttackComponent : AttackComponent
            where TOwnDamageComponent : DamageComponent
        {
            AIAgentType ownAgentType = aiCombatAgent.GetAIAgentType();

            List<uint> visibleRivals = new List<uint>();

            List<AICombatAgentEntity<TRivalContext, TRivalAttackComponent, TRivalDamageComponent>> rivals = 
                ReturnAllRivals<TAgent, TRivalContext, TRivalAttackComponent, TRivalDamageComponent>(ownAgentType);

            float sightMaximumDistance = aiCombatAgent.GetContext().GetSightMaximumDistance();

            foreach (AICombatAgentEntity<TRivalContext, TRivalAttackComponent, TRivalDamageComponent> rival in rivals)
            {
                if (Physics.Raycast(aiCombatAgent.transform.position, rival.transform.position,
                        sightMaximumDistance, _targetsLayerMask[ownAgentType]))
                {
                    continue;
                }

                visibleRivals.Add(rival.GetCombatAgentInstance());
            }

            return visibleRivals;
        }

        public List<float> GetDistancesToGivenEnemies(Vector3 position, List<uint> enemies)
        {
            List<float> distancesToEnemies = new List<float>();

            foreach (uint enemyID in enemies)
            {
                AIEnemy enemy = _aiEnemies[enemyID];
                
                distancesToEnemies.Add((enemy.transform.position - position).magnitude - enemy.GetContext().GetRadius());
            }

            return distancesToEnemies;
        }

        public void CalculateBestAction(AIAlly ally)
        {
            AIAllyAction allyAction = CalculateBestAction<AIAllyAction, AIAllyContext>(ally.GetContext(), _allyUtilityFunction);
            
            CheckIfCanPerformGivenAction<AIAlly, AIAllyContext, AllyAttackComponent, DamageComponent, AIAllyAction>(
                ally, allyAction, AllyPerformAction);
        }

        public void CalculateBestAction(AIEnemy enemy)
        {
            AIEnemyAction enemyAction = CalculateBestAction<AIEnemyAction, AIEnemyContext>(enemy.GetContext(), _enemyUtilityFunction);
            
            CheckIfCanPerformGivenAction<AIEnemy, AIEnemyContext, AttackComponent, AllyDamageComponent, AIEnemyAction>(
                enemy, enemyAction, EnemyPerformAction);
        }

        private static TAction CalculateBestAction<TAction, TContext>(TContext context, 
            IGetBestAction<TAction, TContext> utilityCalculator)
        {
            return utilityCalculator.GetBestAction(context);
        }

        private static void CheckIfCanPerformGivenAction<TAgent, TContext, TAttackComponent, TDamageComponent, TAction>(
            TAgent agent, TAction agentAction, Action<TAgent, TAction> action)
        
            where TAgent : AICombatAgentEntity<TContext, TAttackComponent, TDamageComponent> 
            where TContext : AICombatAgentContext
            where TAttackComponent : AttackComponent
            where TDamageComponent : DamageComponent
            where TAction : Enum
        {
            TContext context = agent.GetContext();
            
            uint agentActionUInt = Convert.ToUInt16(agentAction);
            uint lastAction = context.GetLastActionIndex();

            List<uint> repeatableActions = context.GetRepeatableActions();

            if (agentActionUInt == lastAction && !repeatableActions.Contains(lastAction))
            {
                return;
            }
            
            agent.SetLastActionIndex(agentActionUInt);

            action(agent, agentAction);
        }

        #region Ally

        public List<uint> FilterEnemiesThatTargetsMe(uint combatAgentID, List<uint> visibleRivalsIDs)
        {
            List<uint> enemiesThatTargetsMe = new List<uint>();

            foreach (uint enemyID in visibleRivalsIDs)
            {
                AIEnemy enemy = _aiEnemies[enemyID];
                if (enemy.GetContext().GetRivalID() != combatAgentID)
                {
                    continue;
                }
                enemiesThatTargetsMe.Add(enemyID);
            }

            return enemiesThatTargetsMe;
        }

        private void AllyPerformAction(AIAlly ally, AIAllyAction allyAction)
        {
            _aiAllyActions[allyAction](ally);
        }

        private void AllyFollowPlayer(AIAlly ally)
        {
            //TODO FOLLOW PLAYER
            ShowActionDebugLogs(ally.name + " Following Player");
            //Debug.Log(ally.name + " Following Player");
        }

        private void AllyRequestRival(AIAlly ally)
        {
            ShowActionDebugLogs(ally.name + " Requesting Rival");
            //Debug.Log(ally.name + " Requesting Rival");
            
            List<uint> visibleRivals = ally.GetVisibleRivals();
            
            if (visibleRivals.Count == 0)
            {
                return;
            }

            List<uint> possibleRivals = GetPossibleRivals(visibleRivals);

            if (possibleRivals.Count == 0)
            {
                return;
            }

            uint targetID;

            if (visibleRivals.Count == 1)
            {
                targetID = possibleRivals[0];
            }
            else
            {
                targetID = GetClosestRival<AIEnemy, AIEnemyContext, AttackComponent, AllyDamageComponent>(
                        ally.GetNavMeshAgentComponent().GetTransformComponent(), _aiEnemies, visibleRivals);
            }
            
            AIEnemy targetEnemy = _aiEnemies[targetID];
            
            OnAllySelectTarget(ally, targetEnemy);
        }

        private void OnAllySelectTarget(AIAlly ally, AIEnemy targetEnemy)
        {
            AIEnemyContext targetEnemyContext = targetEnemy.GetContext();

            uint enemyID = targetEnemy.GetCombatAgentInstance();
            
            ally.SetRivalIndex(enemyID);
            ally.SetRivalRadius(targetEnemyContext.GetRadius());
            ally.SetHasATarget(true);
            ally.SetEnemyHealth(targetEnemyContext.GetHealth());
            ally.SetEnemyMaximumStress(targetEnemyContext.GetMaximumStress());
            ally.SetEnemyCurrentStress(targetEnemyContext.GetCurrentStress());
            ally.SetRivalTransform(targetEnemyContext.GetAgentTransform());
        }

        private void AllyGetCloserToEnemy(AIAlly ally)
        {
            ShowActionDebugLogs(ally.name + " Getting Closer To Rival");
            //Debug.Log(ally.name + " Getting Closer To Rival");

            AIEnemy targetEnemy = _aiEnemies[ally.GetContext().GetRivalID()];
            
            ally.ContinueNavigation();
            
            ally.SetDestination(targetEnemy.GetNavMeshAgentComponent().GetTransformComponent());
        }

        private void AllyAttack(AIAlly ally)
        {
            ShowActionDebugLogs(ally.name + " Attacking");
            //Debug.Log(ally.name + " Attacking");
            
            ally.Attack();
        }

        private void AllyFlee(AIAlly ally)
        {
            ShowActionDebugLogs(ally.name + " Fleeing");
            //Debug.Log(ally.name + " Fleeing");
            
            ally.ContinueNavigation();
            
            ally.RequestHelp();
            
            //TODO USE STEERING BEHAVIORS
            
            EvaluateClosestPoint(ally);
        }

        private void AllyDodge(AIAlly ally)
        {
            ShowActionDebugLogs(ally.name + " Dodging");
            //Debug.Log(ally.name + " Dodging");
            
            //TODO REFACTOR TO IFRAMES 
        }

        #endregion
        
        #region Enemy

        private void EnemyPerformAction(AIEnemy enemy, AIEnemyAction enemyAction)
        {
            _aiEnemyActions[enemyAction](enemy);
        }

        private void EnemyPatrol(AIEnemy enemy)
        {
            ShowActionDebugLogs(enemy.name + " Patrolling");
            //Debug.Log(enemy.name + " Patrolling");
            
            //TODO ENEMY PATROL
        }

        private void EnemyRequestRival(AIEnemy enemy)
        {
            ShowActionDebugLogs(enemy.name + " Requesting Rival");
            //Debug.Log(enemy.name + " Requesting Rival");
            
            List<uint> visibleRivals = enemy.GetVisibleRivals();

            if (visibleRivals.Count == 0)
            {
                return;
            }

            uint targetId;

            NavMeshAgentComponent navMeshAgentComponent = enemy.GetNavMeshAgentComponent();

            if (visibleRivals.Count == 1)
            {
                targetId = visibleRivals[0];
            }
            else
            {
                targetId = GetClosestRival<AIAlly, AIAllyContext, AllyAttackComponent, DamageComponent>(
                    navMeshAgentComponent.GetTransformComponent(), _aiAllies, visibleRivals);
            }

            AIAlly targetAlly = _aiAllies[targetId];

            AIAllyContext targetAllyContext = targetAlly.GetContext();
            
            enemy.SetRivalIndex(targetAlly.GetCombatAgentInstance());
            enemy.SetHasATarget(true);
            enemy.SetRivalTransform(targetAllyContext.GetAgentTransform());
        }

        private void EnemyGetCloserToAlly(AIEnemy enemy)
        {
            ShowActionDebugLogs(enemy.name + " Getting Closer To Rival");
            //Debug.Log(enemy.name + " Getting Closer To Rival");

            AIAlly targetEnemy = _aiAllies[enemy.GetContext().GetRivalID()];
            
            enemy.ContinueNavigation();
            
            enemy.SetDestination(targetEnemy.GetNavMeshAgentComponent().GetTransformComponent());
        }

        private void EnemyAttack(AIEnemy enemy)
        {
            ShowActionDebugLogs(enemy.name + " Attacking");
            //Debug.Log(enemy.name + " Attacking");
            
            AttackComponent attackComponent = enemy.Attack();
            
            EnemyStartCastingAnAttack(enemy.transform, attackComponent, enemy);
        }

        private void EnemyFlee(AIEnemy enemy)
        {
            ShowActionDebugLogs(enemy.name + " Fleeing");
            //Debug.Log(enemy.name + " Fleeing");
            
            //TODO ENEMY FLEE
            
            enemy.ContinueNavigation();
        }

        #endregion

        #endregion

        #region Add Combat Agent

        public void AddAIAlly(AIAlly aiAlly)
        {
            _aiAllies.Add(aiAlly.GetCombatAgentInstance(), aiAlly);
        }

        public void AddAIEnemy(AIEnemy aiEnemy)
        {
            _aiEnemies.Add(aiEnemy.GetCombatAgentInstance(), aiEnemy);
            
            AddEnemyAttack(aiEnemy.GetAttackComponents(), GameManager.Instance.GetAllyLayer());
        }

        private void AddEnemyAttack(List<AttackComponent> attackComponents, int layerTarget)
        {
            foreach (AttackComponent attackComponent in attackComponents)
            {
                GameObject colliderObject = null;

                switch (attackComponent.GetAIAttackAoEType())
                {
                    case AIAttackAoEType.RECTANGLE_AREA:
                        colliderObject = Instantiate(_enemyRectangleAttackColliderPrefab);
                        AIEnemyRectangleAttackCollider enemyRectangleAttackCollider = 
                            colliderObject.GetComponent<AIEnemyRectangleAttackCollider>();
                        
                        enemyRectangleAttackCollider.SetRectangleAttackComponent((RectangleAttackComponent)attackComponent);
                        enemyRectangleAttackCollider.SetAttackTargets((int)Mathf.Pow(2, layerTarget));
                        _enemiesAttacksColliders.Add(attackComponent, enemyRectangleAttackCollider);
                        break;

                    case AIAttackAoEType.CIRCLE_AREA:
                        colliderObject = Instantiate(_enemyCircleAttackColliderPrefab);
                        AIEnemyCircleAttackCollider enemyCircleAttackCollider = colliderObject.GetComponent<AIEnemyCircleAttackCollider>();
                        enemyCircleAttackCollider.SetCircleAttackComponent((CircleAttackComponent)attackComponent);
                        enemyCircleAttackCollider.SetAttackTargets((int)Mathf.Pow(2, layerTarget));
                        _enemiesAttacksColliders.Add(attackComponent, enemyCircleAttackCollider);
                        break;

                    case AIAttackAoEType.CONE_AREA:
                        AIEnemyConeAttackCollider enemyConeAttackCollider = colliderObject.GetComponent<AIEnemyConeAttackCollider>();
                        enemyConeAttackCollider.SetConeAttackComponent((ConeAttackComponent)attackComponent);
                        enemyConeAttackCollider.SetAttackTargets((int)Mathf.Pow(2, layerTarget));
                        _enemiesAttacksColliders.Add(attackComponent, enemyConeAttackCollider);
                        break;
                }

                colliderObject.SetActive(false);
            }
        }

        #endregion

        #region Combat Agents Events

        private List<uint> GetPossibleRivals(List<uint> visibleRivals)
        {
            List<uint> auxVisibleRivals = new List<uint>();
            
            auxVisibleRivals.AddRange(visibleRivals);
            
            foreach (AIAlly otherAlly in _aiAllies.Values)
            {
                for (int i = visibleRivals.Count - 1; i >= 0; i--)
                {
                    uint enemyID = visibleRivals[i];
                    
                    if (otherAlly.GetContext().GetRivalID() != enemyID)
                    {
                        continue;
                    }
                    
                    visibleRivals.RemoveAt(i);
                }
            }

            return visibleRivals.Count == 0 ? auxVisibleRivals : visibleRivals;
        }

        private uint GetClosestRival<TRivalAgent, TRivalContext, TRivalAttackComponent, TRivalDamageComponent>(
            IPosition positionComponent, Dictionary<uint, TRivalAgent> rivalsDictionary, List<uint> possibleTargetsAICombatAgentIDs)
        
            where TRivalAgent : AICombatAgentEntity<TRivalContext, TRivalAttackComponent, TRivalDamageComponent>
            where TRivalContext : AICombatAgentContext
            where TRivalAttackComponent : AttackComponent
            where TRivalDamageComponent : DamageComponent
        {
            uint targetID = 0;
            TRivalAgent currentTarget;

            float targetDistance = 300000000;
            float currentTargetDistance;

            for (int i = 0; i < possibleTargetsAICombatAgentIDs.Count; i++)
            {
                currentTarget = rivalsDictionary[possibleTargetsAICombatAgentIDs[i]];

                currentTargetDistance = (currentTarget.transform.position - positionComponent.GetPosition()).magnitude;

                if (currentTargetDistance >= targetDistance)
                {
                    continue;
                }

                targetID = currentTarget.GetCombatAgentInstance();
                targetDistance = currentTargetDistance;
            }

            return targetID;
        }
        
        public void OnAllyDefeated(AIAlly aiAlly)
        {
            //TODO ON ALLY DEFEATED
            
            OnAgentDefeated<AIEnemy, AIEnemyContext, AttackComponent, AllyDamageComponent, 
                AIAllyContext, AllyAttackComponent, DamageComponent>(aiAlly, ref _aiEnemies);
        }

        public void OnEnemyReceiveDamage(uint enemyAgentInstanceID, uint enemyHealth, float enemyStress, bool isStunned)
        {
            foreach (AIAlly ally in _aiAllies.Values)
            {
                if (ally.GetContext().GetRivalID() != enemyAgentInstanceID)
                {
                    continue;
                }

                ally.SetEnemyHealth(enemyHealth);
                ally.SetEnemyCurrentStress(enemyStress);
                ally.SetIsEnemyStunned(isStunned);
            }
        }

        public void OnEnemyStunEnds(uint enemyAgentInstanceID)
        {
            foreach (AIAlly ally in _aiAllies.Values)
            {
                if (ally.GetContext().GetRivalID() != enemyAgentInstanceID)
                {
                    continue;
                }
                
                ally.SetIsEnemyStunned(false);
            }
        }

        public void OnEnemyDefeated(AIEnemy aiEnemy)
        {
            uint combatAgentInstance = aiEnemy.GetCombatAgentInstance();

            _aiEnemies.Remove(combatAgentInstance);
            
            OnAgentDefeated<AIAlly, AIAllyContext, AllyAttackComponent, DamageComponent, 
                AIEnemyContext, AttackComponent, AllyDamageComponent>(aiEnemy, ref _aiAllies);
        }

        private void OnAgentDefeated<TAgent, TRivalContext, TRivalAttackComponent, TRivalDamageComponent, 
            TOwnContext, TOwnAttackComponent, TOwnDamageComponent>(
            AICombatAgentEntity<TOwnContext, TOwnAttackComponent, TOwnDamageComponent> aiCombatAgentDefeated, 
            ref Dictionary<uint, TAgent> agents)
        
            where TAgent : AICombatAgentEntity<TRivalContext, TRivalAttackComponent, TRivalDamageComponent>
            where TRivalContext : AICombatAgentContext
            where TRivalAttackComponent : AttackComponent
            where TRivalDamageComponent : DamageComponent
            where TOwnContext : AICombatAgentContext
            where TOwnAttackComponent : AttackComponent
            where TOwnDamageComponent : DamageComponent
        {
            foreach (TAgent agent in agents.Values)
            {
                if (agent.GetContext().GetRivalID() != aiCombatAgentDefeated.GetCombatAgentInstance())
                {
                    continue;
                }
                
                agent.GetContext().SetHasATarget(false);
                ECSNavigationManager.Instance.UpdateNavMeshAgentTransformDestination(agent.GetNavMeshAgentComponent(), null);
            }
        }

        #endregion

        #region Flee Events

        public void RequestSafeSpot(AICombatAgentEntity<AIAllyContext, AllyAttackComponent, DamageComponent> aiCombatAgentEntity)
        {
            
        }

        #endregion

        #region Attack Events

        private void EnemyStartCastingAnAttack(Transform attackerTransform, 
            AttackComponent attackComponent, AIEnemy enemy)
        {
            if (attackComponent.IsOnCooldown())
            {
                enemy.NotAttacking();
                return;
            }
            
            AIAttackCollider attackCollider = _enemiesAttacksColliders[attackComponent];
            attackCollider.SetParent(attackerTransform);
            attackCollider.gameObject.SetActive(true);
            StartCoroutine(StartEnemyAttackCastTimeCoroutine(attackComponent, attackCollider, enemy));
        }

        private void PutAttackOnCooldown(AttackComponent attackComponent, AIEnemy enemy)
        {
            enemy.NotAttacking();
            StartCoroutine(StartCooldownCoroutine(attackComponent, enemy));
        }

        #endregion

        #region Systems
        
        #region Combat System

        private void UpdateDistanceToRival<TOwnContext, TOwnAttackComponent, TOwnDamageComponent>(
            AICombatAgentEntity<TOwnContext, TOwnAttackComponent, TOwnDamageComponent> combatAgent)
            
            where TOwnContext : AICombatAgentContext
            where TOwnAttackComponent : AttackComponent
            where TOwnDamageComponent : DamageComponent
        {
            Vector3 vectorToRival = combatAgent.GetContext().GetRivalTransform().position - combatAgent.transform.position;
            
            combatAgent.SetVectorToRival(vectorToRival);
            combatAgent.SetDistanceToRival(vectorToRival.magnitude);
        }

        #endregion
        
        #region Attack System

        private IEnumerator StartEnemyAttackCastTimeCoroutine(AttackComponent attackComponent, AIAttackCollider attackCollider, 
            AIEnemy enemy)
        {
            attackComponent.StartCastTime();
            while (attackComponent.IsCasting() && !enemy.GetContext().IsStunned())
            {
                attackComponent.DecreaseCurrentCastTime();
                yield return null;
            }

            if (!enemy.GetContext().IsStunned())
            {
                attackCollider.StartInflictingDamage();

                if (attackComponent.DoesDamageOverTime())
                {
                    StartCoroutine(StartDamageOverTime(attackComponent, attackCollider, enemy));
                    yield break;
                }
            
                enemy.RotateToNextPathCorner();
            }
            
            Instance.PutAttackOnCooldown(attackComponent, enemy);
            attackCollider.Deactivate();
        }

        private IEnumerator StartDamageOverTime(AttackComponent attackComponent, AIAttackCollider attackCollider, 
            AIEnemy enemy)
        {
            while (attackComponent.DidDamageOverTimeFinished())
            {
                attackComponent.DecreaseRemainingTimeDealingDamage();
                yield return null;
            }
           
            enemy.RotateToNextPathCorner();
            Instance.PutAttackOnCooldown(attackComponent, enemy);
            attackCollider.Deactivate();
        }

        private IEnumerator StartCooldownCoroutine(AttackComponent attackComponent, AIEnemy enemy)
        {
            attackComponent.StartCooldown();
            while (attackComponent.IsOnCooldown())
            {
                attackComponent.DecreaseCooldown();
                yield return null;
            }
            
            enemy.OnAttackAvailableAgain(attackComponent);
        }
        
        #endregion
        
        #region Flee System
        
        //ERASE!!!!
        private List<Vector3> GetTerrainPositions(List<GameObject> FLEE_POINTS)
        {
            List<Vector3> points = new List<Vector3>();
            
            RaycastHit hit;

            foreach (GameObject gameObject in FLEE_POINTS)
            {
                Ray ray = new Ray(gameObject.transform.position, Vector3.down);
                if (Physics.Raycast(ray, out hit))
                {
                    points.Add(hit.point);
                }
            }

            return points;
        }
        
        private void EvaluateClosestPoint(AIAlly combatAgentNeedsToFlee)
        {
            if (FLEE_POINTS_RECORD.ContainsKey(combatAgentNeedsToFlee))
            {
                return;
            }
            
            Vector3 agentPosition = combatAgentNeedsToFlee.transform.position;
         
            Vector3 destination = TERRAIN_POSITIONS[0];            
            float closestDistance = (destination - agentPosition).magnitude;

            int index = 0;

            for (int i = 0; i < TERRAIN_POSITIONS.Count; i++)
            {
                Vector3 currentPosition = TERRAIN_POSITIONS[i];
                
                float currentDistance = (agentPosition - currentPosition).magnitude;
                if (currentDistance > closestDistance)
                {
                    continue;
                }

                closestDistance = currentDistance;
                destination = currentPosition;
                index = i;
            }
            
            FLEE_POINTS_RECORD.Add(combatAgentNeedsToFlee, index);
            
            combatAgentNeedsToFlee.SetDestination(new VectorComponent(destination));
        } 

        private IEnumerator UpdateFleeMovement()
        {
            while (true)
            {
                Dictionary<AIAlly, int> newIndexes = new Dictionary<AIAlly, int>(); 
                
                foreach (var combatAgentFleeing in FLEE_POINTS_RECORD)
                {
                    AIAlly combatAgent = combatAgentFleeing.Key;
                    int index = combatAgentFleeing.Value;

                    if ((combatAgent.transform.position - TERRAIN_POSITIONS[index]).magnitude < 8)
                    {
                        int newIndex = (combatAgentFleeing.Value + 1) % TERRAIN_POSITIONS.Count;
                        
                        newIndexes.Add(combatAgent, newIndex);
                    }
                }

                foreach (var VARIABLE in newIndexes)
                {
                    AIAlly combatAgent = VARIABLE.Key;
                    int newIndex = VARIABLE.Value;

                    FLEE_POINTS_RECORD[combatAgent] = newIndex;
                    
                    combatAgent.SetDestination(new VectorComponent(TERRAIN_POSITIONS[newIndex]));
                }

                yield return null;
            }
        }
        //

        #endregion

        #region Dodge System

        private Vector2 GetNearestPointToDodge(List<Vector2> polygon, Vector2 point, float radius)
        {
            Vector2 closestPoint = Vector2.zero;
            float shortestDistance = Mathf.Infinity;

            Vector2 start;
            Vector2 end;
            Vector2 direction = new Vector2();
            Vector2 closestPointOnEdge = new Vector2();

            for (int i = 0; i < polygon.Count; i++)
            {
                start = polygon[i];
                end = polygon[(i + 1) % polygon.Count];

                direction = end - start;
                
                closestPointOnEdge = GetClosestPointOnLineSegment(start, end, point);

                float distance = Vector2.Distance(point, closestPointOnEdge);

                if (distance >= shortestDistance)
                {
                    continue;
                }
                
                shortestDistance = distance;
                closestPoint = closestPointOnEdge + (new Vector2(direction.y, -direction.x).normalized * (radius * 1.5f));
            }

            return closestPoint;
        }
        
        private Vector2 GetClosestPointOnLineSegment(Vector2 start, Vector2 end, Vector2 point)
        {
            Vector2 line = end - start;
            float lengthSquared = line.sqrMagnitude;

            float distance = Vector2.Dot(point - start, line) / lengthSquared;
            distance = Mathf.Clamp01(distance);

            return start + distance * line;
        }

        #endregion
        
        #endregion

        private List<TAgent> ReturnAllDictionaryValuesInAList<TAgent, TOwnContext, TOwnAttackComponent, TOwnDamageComponent>(
            Dictionary<uint, TAgent> agentsDictionary)
        
            where TAgent : AICombatAgentEntity<TOwnContext, TOwnAttackComponent, TOwnDamageComponent>
            where TOwnContext : AICombatAgentContext
            where TOwnAttackComponent : AttackComponent
            where TOwnDamageComponent : DamageComponent
        {
            List<TAgent> agentsList = new List<TAgent>();

            foreach (TAgent combatAgent in agentsDictionary.Values)
            {
                agentsList.Add(combatAgent);
            }

            return agentsList;
        }

        private List<AICombatAgentEntity<TOwnContext, TOwnAttackComponent, TOwnDamageComponent>> 
            ReturnAllRivals<TAgent, TOwnContext, TOwnAttackComponent, TOwnDamageComponent>(AIAgentType aiAgentType)
        
            where TAgent : AICombatAgentEntity<TOwnContext, TOwnAttackComponent, TOwnDamageComponent>
            where TOwnContext : AICombatAgentContext
            where TOwnAttackComponent : AttackComponent
            where TOwnDamageComponent : DamageComponent
        {
            List<AICombatAgentEntity<TOwnContext, TOwnAttackComponent, TOwnDamageComponent>> combatAgents = 
                new List<AICombatAgentEntity<TOwnContext, TOwnAttackComponent, TOwnDamageComponent>>();

            for (AIAgentType i = 0; i < AIAgentType.ENUM_SIZE; i++)
            {
                if (aiAgentType == i)
                {
                    continue;
                }

                if (!_returnTheSameAgentsType.ContainsKey(i))
                {
                    continue;
                }

                List<TAgent> currentCombatAgents = ExecuteDelegate<List<TAgent>>(i);

                if (currentCombatAgents != null)
                {
                    combatAgents.AddRange(currentCombatAgents);    
                }
            }

            return combatAgents;
        }
        
        private T ExecuteDelegate<T>(AIAgentType agentType)
        {
            Delegate del = _returnTheSameAgentsType[agentType];
            
            if (del is Func<T> func)
            {
                return func();
            }
            return default;
        }

        private List<T> UnifyArraysInAList<T>(T[] firstArray, T[] secondArray)
        {
            List<T> list = new List<T>();
            
            list.AddRange(firstArray);

            foreach (T t in secondArray)
            {
                if (list.Contains(t))
                {
                    continue;
                }
                
                list.Add(t);
            }

            return list;
        }

        private List<T> UnifyLists<T>(List<T> firstList, List<T> secondList)
        {
            foreach (T t in secondList)
            {
                if (firstList.Contains(t))
                {
                    continue;
                }    
                firstList.Add(t);
            }
            
            return firstList;
        }

        private List<T> SubtractLists<T>(List<T> firstList, List<T> secondList)
        {
            foreach (T t in secondList)
            {
                if (!firstList.Contains(t))
                {
                    continue;
                }

                firstList.Remove(t);
            }
            
            return firstList;
        }
    }
}