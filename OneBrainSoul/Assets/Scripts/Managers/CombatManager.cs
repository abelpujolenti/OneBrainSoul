using System;
using System.Collections.Generic;
using AI;
using AI.Combat.Ally;
using AI.Combat.AttackColliders;
using AI.Combat.Enemy;
using AI.Combat.ScriptableObjects;
using ECS.Components.AI.Combat;
using ECS.Components.AI.Navigation;
using ECS.Entities.AI.Combat;
using Interfaces.AI.Navigation;
using UnityEngine;

namespace Managers
{
    public class CombatManager : MonoBehaviour
    {
        private static CombatManager _instance;

        public static CombatManager Instance => _instance;

        [SerializeField] private GameObject _enemyRectangleAttackColliderPrefab;
        [SerializeField] private GameObject _enemyCircleAttackColliderPrefab;
        
        private Dictionary<uint, AIAlly> _aiAllies = new Dictionary<uint, AIAlly>();
        private Dictionary<uint, AIEnemy> _aiEnemies = new Dictionary<uint, AIEnemy>();
        
        private readonly Dictionary<AIAgentType, Delegate> _returnDictionaryOfTheSameType = new Dictionary<AIAgentType, Delegate>
        {
            {
                AIAgentType.ALLY, new Func<Dictionary<uint, AIAlly>>(() => _instance._aiAllies)
            },
            {
                AIAgentType.ENEMY, new Func<Dictionary<uint, AIEnemy>>(() => _instance._aiEnemies)
            },
        };

        private readonly Dictionary<AIAgentType, Delegate> _returnTheSameAgentsType = new Dictionary<AIAgentType, Delegate>
        {
            { 
                AIAgentType.ALLY, new Func<List<AIAlly>>(() => 
                _instance.ReturnAllDictionaryValuesInAList<AIAlly ,AIAllyContext, AllyAttackComponent, DamageComponent, 
                    AIAllyAction>(_instance._aiAllies)) 
            },
            { 
                AIAgentType.ENEMY, new Func<List<AIEnemy>>(() => 
                _instance.ReturnAllDictionaryValuesInAList<AIEnemy, AIEnemyContext, AttackComponent, AllyDamageComponent, 
                    AIEnemyAction>(_instance._aiEnemies)) 
            }
        };

        private readonly Dictionary<AIAgentType, int> _targetsLayerMask = new Dictionary<AIAgentType, int>
        {
            { AIAgentType.ALLY, (int)(Math.Pow(2, 7) + Math.Pow(2, 6)) },
            { AIAgentType.ENEMY, (int)(Math.Pow(2, 8) + Math.Pow(2, 6)) }
        };

        private Dictionary<AttackComponent, AIEnemyAttackCollider> _enemiesAttacksColliders =
            new Dictionary<AttackComponent, AIEnemyAttackCollider>();

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;

                DontDestroyOnLoad(gameObject);

                return;
            }

            Destroy(gameObject);
        }

        #region UBS
        
        public List<uint> GetVisibleRivals<TAgent, TRivalContext, TRivalAttackComponent, TRivalDamageComponent, TRivalAction, 
            TOwnContext, TOwnAttackComponent, TOwnDamageComponent, TOwnAction>(
            AICombatAgentEntity<TOwnContext, TOwnAttackComponent, TOwnDamageComponent, TOwnAction> aiCombatAgent)
        
            where TAgent : AICombatAgentEntity<TRivalContext, TRivalAttackComponent, TRivalDamageComponent, TRivalAction>
            where TRivalContext : AICombatAgentContext
            where TRivalAttackComponent : AttackComponent
            where TRivalDamageComponent : DamageComponent
            where TRivalAction : Enum
            where TOwnContext : AICombatAgentContext
            where TOwnAttackComponent : AttackComponent
            where TOwnDamageComponent : DamageComponent
            where TOwnAction : Enum
        {
            AIAgentType ownAgentType = aiCombatAgent.GetAIAgentType();

            List<uint> visibleRivals = new List<uint>();

            List<TAgent> rivals = ReturnAllRivals<TAgent, TRivalContext, TRivalAttackComponent, TRivalDamageComponent, TRivalAction>(ownAgentType);

            float sightMaximumDistance = aiCombatAgent.GetContext().GetSightMaximumDistance();

            Vector3 position;
            Vector3 vectorToEnemy;

            float distanceToEnemy;

            foreach (TAgent rival in rivals)
            {
                position = aiCombatAgent.transform.position;
                vectorToEnemy = (rival.transform.position - position).normalized;

                distanceToEnemy = vectorToEnemy.magnitude;

                if (distanceToEnemy > sightMaximumDistance)
                {
                    continue;
                }
                
                if (Physics.Raycast(position, vectorToEnemy, distanceToEnemy, _targetsLayerMask[ownAgentType]))
                {
                    continue;
                }

                visibleRivals.Add(rival.GetAgentID());
            }

            return visibleRivals;
        }

        public (List<Vector3>, List<float>) GetVectorsAndDistancesToGivenEnemies(Vector3 position, List<uint> enemies)
        {
            List<Vector3> vectorsToEnemies = new List<Vector3>();
            List<float> distancesToEnemies = new List<float>();

            foreach (uint enemyID in enemies)
            {
                AIEnemy enemy = _aiEnemies[enemyID];

                Vector3 vector = enemy.transform.position - position;
                
                vectorsToEnemies.Add(vector);
                
                distancesToEnemies.Add(vector.magnitude - enemy.GetContext().GetRadius());
            }

            return (vectorsToEnemies, distancesToEnemies);
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

        #endregion

        #endregion

        #region Add Combat Agent

        public void AddAIAlly(AIAlly aiAlly)
        {
            uint agentID = aiAlly.GetAgentID();
            
            _aiAllies.Add(agentID, aiAlly);
        }

        public void AddAIEnemy(AIEnemy aiEnemy)
        {
            uint agentID = aiEnemy.GetAgentID();
            
            _aiEnemies.Add(agentID, aiEnemy);
        }

        #endregion

        #region Combat Agents Events

        public List<uint> GetPossibleRivals(List<uint> visibleRivals)
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

        public uint GetClosestRivalID<TRivalAgent, TRivalContext, TRivalAttackComponent, TRivalDamageComponent, TRivalAction>(
            IPosition positionComponent, List<uint> possibleTargetsAICombatAgentIDs, AIAgentType agentType)
        
            where TRivalAgent : AICombatAgentEntity<TRivalContext, TRivalAttackComponent, TRivalDamageComponent, TRivalAction>
            where TRivalContext : AICombatAgentContext
            where TRivalAttackComponent : AttackComponent
            where TRivalDamageComponent : DamageComponent
            where TRivalAction : Enum
        {
            
            Dictionary<uint, TRivalAgent> rivalsDictionary = 
                ReturnAgentTypeDictionary<TRivalAgent, TRivalContext, TRivalAttackComponent, TRivalDamageComponent, TRivalAction>(agentType);
            
            uint targetID = 0;
            TRivalAgent currentTarget;

            float targetDistance = Mathf.Infinity;
            float currentTargetDistance;

            for (int i = 0; i < possibleTargetsAICombatAgentIDs.Count; i++)
            {
                currentTarget = rivalsDictionary[possibleTargetsAICombatAgentIDs[i]];

                currentTargetDistance = (currentTarget.transform.position - positionComponent.GetPosition()).magnitude;

                if (currentTargetDistance >= targetDistance)
                {
                    continue;
                }

                targetID = currentTarget.GetAgentID();
                targetDistance = currentTargetDistance;
            }

            return targetID;
        }
        
        public void OnAllyDefeated(AIAlly aiAlly)
        {
            uint agentID = aiAlly.GetAgentID();
            
            foreach (AIEnemy enemy in _aiEnemies.Values)
            {
                if (enemy.GetContext().GetRivalID() != agentID)
                {
                    continue;
                }
                
                enemy.SetIsDueling(false);
            }
            
            OnAgentDefeated<AIEnemy, AIEnemyContext, AttackComponent, AllyDamageComponent, AIEnemyAction, 
                AIAllyContext, AllyAttackComponent, DamageComponent, AIAllyAction>(aiAlly, ref _aiEnemies);
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
            _aiEnemies.Remove(aiEnemy.GetAgentID());
            
            OnAgentDefeated<AIAlly, AIAllyContext, AllyAttackComponent, DamageComponent, AIAllyAction, 
                AIEnemyContext, AttackComponent, AllyDamageComponent, AIEnemyAction>(aiEnemy, ref _aiAllies);
        }

        private void OnAgentDefeated<TAgent, TRivalContext, TRivalAttackComponent, TRivalDamageComponent, TRivalAction, 
            TOwnContext, TOwnAttackComponent, TOwnDamageComponent, TOwnAction>(
            AICombatAgentEntity<TOwnContext, TOwnAttackComponent, TOwnDamageComponent, TOwnAction> aiCombatAgentDefeated, 
            ref Dictionary<uint, TAgent> agents)
        
            where TAgent : AICombatAgentEntity<TRivalContext, TRivalAttackComponent, TRivalDamageComponent, TRivalAction>
            where TRivalContext : AICombatAgentContext
            where TRivalAttackComponent : AttackComponent
            where TRivalDamageComponent : DamageComponent
            where TRivalAction : Enum
            where TOwnContext : AICombatAgentContext
            where TOwnAttackComponent : AttackComponent
            where TOwnDamageComponent : DamageComponent
            where TOwnAction : Enum
        {
            foreach (TAgent agent in agents.Values)
            {
                if (agent.GetContext().GetRivalID() != aiCombatAgentDefeated.GetAgentID())
                {
                    continue;
                }
                
                agent.GetContext().SetHasATarget(false);
                ECSNavigationManager.Instance.UpdateNavMeshAgentDestination(agent.GetAgentID(), 
                    new VectorComponent(agent.GetNavMeshAgentComponent().GetTransformComponent().GetPosition()));
            }
        }

        #endregion

        #region Systems
        
        #region Combat System

        #endregion
        
        #region Attack System
        
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

        public AIAlly RequestAlly(uint agentID)
        {
            if (!_aiAllies.ContainsKey(agentID))
            {
                return null;
            }
            
            return _aiAllies[agentID];
        }

        public AIEnemy RequestEnemy(uint agentID)
        {
            if (!_aiEnemies.ContainsKey(agentID))
            {
                return null;
            }
            
            return _aiEnemies[agentID];
        }

        #region Collections Methods

        private Dictionary<uint, TAgent>
            ReturnAgentTypeDictionary<TAgent, TContext, TAttackComponent, TDamageComponent, TAction>(AIAgentType aiAgentType)
        
            where TAgent : AICombatAgentEntity<TContext, TAttackComponent, TDamageComponent, TAction>
            where TContext : AICombatAgentContext
            where TAttackComponent : AttackComponent
            where TDamageComponent : DamageComponent
            where TAction : Enum
        {
            return ExecuteDelegate<Dictionary<uint, TAgent>, Dictionary<AIAgentType, Delegate>>
                (_returnDictionaryOfTheSameType, aiAgentType);
        }

        private List<TAgent> 
            ReturnAllRivals<TAgent, TContext, TAttackComponent, TDamageComponent, TAction>(AIAgentType aiAgentType)
        
            where TAgent : AICombatAgentEntity<TContext, TAttackComponent, TDamageComponent, TAction>
            where TContext : AICombatAgentContext
            where TAttackComponent : AttackComponent
            where TDamageComponent : DamageComponent
            where TAction : Enum
        {
            List<TAgent> combatAgents = new List<TAgent>();

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

                List<TAgent> currentCombatAgents = 
                    ExecuteDelegate<List<TAgent>, Dictionary<AIAgentType, Delegate>>(_returnTheSameAgentsType, i);

                if (currentCombatAgents != null)
                {
                    combatAgents.AddRange(currentCombatAgents);    
                }
            }

            return combatAgents;
        }

        private List<TAgent> ReturnAllDictionaryValuesInAList<TAgent, TContext, TAttackComponent, TDamageComponent, 
            TAction>(Dictionary<uint, TAgent> agentsDictionary)
        
            where TAgent : AICombatAgentEntity<TContext, TAttackComponent, TDamageComponent, TAction>
            where TContext : AICombatAgentContext
            where TAttackComponent : AttackComponent
            where TDamageComponent : DamageComponent
            where TAction : Enum
        {
            List<TAgent> agentsList = new List<TAgent>();

            foreach (TAgent combatAgent in agentsDictionary.Values)
            {
                agentsList.Add(combatAgent);
            }

            return agentsList;
        }
        
        private TReturn ExecuteDelegate<TReturn, TCollection>(TCollection collection, AIAgentType agentType)
            where TCollection : Dictionary<AIAgentType, Delegate>
        {
            Delegate del = collection[agentType];
            
            if (del is Func<TReturn> func)
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

        #endregion
    }
}