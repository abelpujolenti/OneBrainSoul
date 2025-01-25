using System;
using System.Collections.Generic;
using AI.Combat.AbilityColliders;
using AI.Combat.Contexts;
using AI.Combat.Enemy;
using AI.Combat.Enemy.LongArms;
using AI.Combat.Enemy.Triface;
using ECS.Components.AI.Combat;
using ECS.Entities.AI.Combat;
using Player;
using UnityEngine;

namespace Managers
{
    public class CombatManager : MonoBehaviour
    {
        private static CombatManager _instance;

        public static CombatManager Instance => _instance;

        [SerializeField] private GameObject _enemyRectangleAttackColliderPrefab;
        [SerializeField] private GameObject _enemyCircleAttackColliderPrefab;

        private PlayerStatus _playerStatus;
        
        private Dictionary<uint, Triface> _trifaces = new Dictionary<uint, Triface>();
        private Dictionary<uint, LongArms> _longArms = new Dictionary<uint, LongArms>();
        
        private readonly Dictionary<AIEnemyType, Delegate> _returnDictionaryOfTheSameType = new Dictionary<AIEnemyType, Delegate>
        {
            {
                AIEnemyType.TRIFACE, new Func<Dictionary<uint, Triface>>(() => _instance._trifaces)
            },
            {
                AIEnemyType.LONG_ARMS, new Func<Dictionary<uint, LongArms>>(() => _instance._longArms)
            },
        };

        private readonly Dictionary<AIEnemyType, Delegate> _returnTheSameAgentsType = new Dictionary<AIEnemyType, Delegate>
        {
            { 
                AIEnemyType.TRIFACE, new Func<List<Triface>>(() => 
                _instance.ReturnAllDictionaryValuesInAList<Triface, TrifaceContext, TrifaceAction>(_instance._trifaces)) 
            },
            { 
                AIEnemyType.LONG_ARMS, new Func<List<LongArms>>(() => 
                _instance.ReturnAllDictionaryValuesInAList<LongArms, LongArmsContext, LongArmsAction>(_instance._longArms)) 
            }
        };

        private Dictionary<AttackComponent, AIEnemyAbilityCollider> _enemiesAttacksColliders =
            new Dictionary<AttackComponent, AIEnemyAbilityCollider>();

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

        public bool CanSeePlayer(Vector3 position, float sightMaximumDistance)
        {
            Vector3 playerPosition = _playerStatus.GetPosition();

            Vector3 vectorToPlayer = (playerPosition - position).normalized;
            float distanceToPlayer = (playerPosition - position).sqrMagnitude;

            if (distanceToPlayer > sightMaximumDistance * sightMaximumDistance)
            {
                return false;
            }
            
            return !Physics.Raycast(position, vectorToPlayer, distanceToPlayer, (int)(Math.Pow(2, 8) + Math.Pow(2, 6)));
        }

        #endregion

        #region Add Combat Agent

        public void AddPlayer(PlayerStatus playerStatus)
        {
            _playerStatus = playerStatus;
        }

        public void AddEnemy(Triface triface)
        {
            uint agentID = triface.GetAgentID();
            
            _trifaces.Add(agentID, triface);
        }

        public void AddEnemy(LongArms longArms)
        {
            uint agentID = longArms.GetAgentID();
            
            _longArms.Add(agentID, longArms);
        }

        #endregion

        #region Combat Agents Events

        public void OnEnemyDefeated(Triface triface)
        {
            _trifaces.Remove(triface.GetAgentID());
        }

        public void OnEnemyDefeated(LongArms longArms)
        {
            _longArms.Remove(longArms.GetAgentID());
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

        #region Collections Methods

        private Dictionary<uint, TAgent> ReturnAgentTypeDictionary<TAgent, TContext, TAction>(AIEnemyType aiEnemyType)
        
            where TAgent : AIEnemy<TContext, TAction>
            where TContext : AIEnemyContext
            where TAction : Enum
        {
            return ExecuteDelegate<Dictionary<uint, TAgent>, Dictionary<AIEnemyType, Delegate>>
                (_returnDictionaryOfTheSameType, aiEnemyType);
        }

        private List<TAgent> ReturnAllDictionaryValuesInAList<TAgent, TContext, TAction>
            (Dictionary<uint, TAgent> agentsDictionary)
        
            where TAgent : AIEnemy<TContext, TAction>
            where TContext : AIEnemyContext
            where TAction : Enum
        {
            List<TAgent> agentsList = new List<TAgent>();

            foreach (TAgent combatAgent in agentsDictionary.Values)
            {
                agentsList.Add(combatAgent);
            }

            return agentsList;
        }

        private List<TAgent> ReturnAllRivals<TAgent, TContext, TAction>(AIEnemyType aiEnemyType)
        
            where TAgent : AIEnemy<TContext, TAction>
            where TContext : AIEnemyContext
            where TAction : Enum
        {
            List<TAgent> combatAgents = new List<TAgent>();

            for (AIEnemyType i = 0; i < AIEnemyType.ENUM_SIZE; i++)
            {
                if (aiEnemyType == i)
                {
                    continue;
                }

                if (!_returnTheSameAgentsType.ContainsKey(i))
                {
                    continue;
                }

                List<TAgent> currentCombatAgents = 
                    ExecuteDelegate<List<TAgent>, Dictionary<AIEnemyType, Delegate>>(_returnTheSameAgentsType, i);

                if (currentCombatAgents != null)
                {
                    combatAgents.AddRange(currentCombatAgents);    
                }
            }

            return combatAgents;
        }
        
        private TReturn ExecuteDelegate<TReturn, TCollection>(TCollection collection, AIEnemyType aiEnemyType)
            where TCollection : Dictionary<AIEnemyType, Delegate>
        {
            Delegate del = collection[aiEnemyType];
            
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