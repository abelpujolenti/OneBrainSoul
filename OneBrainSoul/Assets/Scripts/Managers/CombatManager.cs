using System;
using System.Collections.Generic;
using AI.Combat.Enemy;
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
        private Dictionary<uint, LongArmsBase> _longArmsBases = new Dictionary<uint, LongArmsBase>();
        
        private readonly Dictionary<EnemyType, Delegate> _returnDictionaryOfTheSameType = new Dictionary<EnemyType, Delegate>
        {
            {
                EnemyType.TRIFACE, new Func<Dictionary<uint, Triface>>(() => _instance._trifaces)
            },
            {
                EnemyType.LONG_ARMS, new Func<Dictionary<uint, LongArms>>(() => _instance._longArms)
            },
            {
                EnemyType.LONG_ARMS_BASE, new Func<Dictionary<uint, LongArmsBase>>(() => _instance._longArmsBases)
            }
        };

        private readonly Dictionary<EnemyType, Delegate> _returnTheSameAgentsType = new Dictionary<EnemyType, Delegate>
        {
            { 
                EnemyType.TRIFACE, new Func<List<Triface>>(() => 
                _instance.ReturnAllDictionaryValuesInAList<Triface>(_instance._trifaces)) 
            },
            { 
                EnemyType.LONG_ARMS, new Func<List<LongArms>>(() => 
                _instance.ReturnAllDictionaryValuesInAList<LongArms>(_instance._longArms)) 
            },
            { 
                EnemyType.LONG_ARMS_BASE, new Func<List<LongArmsBase>>(() => 
                _instance.ReturnAllDictionaryValuesInAList<LongArmsBase>(_instance._longArmsBases)) 
            }
        };

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
            Vector3 playerPosition = _playerStatus.GetTransformComponent().GetPosition();

            Vector3 vectorToPlayer = (playerPosition - position).normalized;
            float distanceToPlayer = (playerPosition - position).magnitude;

            if (distanceToPlayer > sightMaximumDistance)
            {
                return false;
            }

            return !Physics.Raycast(position, vectorToPlayer, distanceToPlayer, 
                GameManager.Instance.GetEnemyLayer() + GameManager.Instance.GetGroundLayer());
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

        public void AddEnemy(LongArmsBase longArmsBase)
        {
            uint agentID = (uint)longArmsBase.GetInstanceID();
            
            _longArmsBases.Add(agentID, longArmsBase);
        }

        #endregion

        #region Requests

        public PlayerStatus RequestPlayerStatus()
        {
            return _playerStatus;
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

        public GameObject GetRectanglePrefab()
        {
            return _enemyRectangleAttackColliderPrefab;
        }

        public GameObject GetCirclePrefab()
        {
            return _enemyCircleAttackColliderPrefab;
        }

        #region Collections Methods

        /*private Dictionary<uint, TAgent> ReturnAgentTypeDictionary<TAgent, TContext, TAction, TAbility>(AIEnemyType aiEnemyType)
        
            where TAgent : AIEnemy<TContext, TAction, TAbility>
            where TContext : AIEnemyContext
            where TAction : Enum
            where TAbility : Enum
        {
            return ExecuteDelegate<Dictionary<uint, TAgent>, Dictionary<AIEnemyType, Delegate>>
                (_returnDictionaryOfTheSameType, aiEnemyType);
        }*/

        private Dictionary<uint, T> ReturnAgentTypeDictionary<T>(EnemyType enemyType)
        {
            return ExecuteDelegate<Dictionary<uint, T>, Dictionary<EnemyType, Delegate>>
                (_returnDictionaryOfTheSameType, enemyType);
        }

        /*private List<TAgent> ReturnAllDictionaryValuesInAList<TAgent, TContext, TAction, TAbility>
            (Dictionary<uint, TAgent> agentsDictionary)
        
            where TAgent : AIEnemy<TContext, TAction, TAbility>
            where TContext : AIEnemyContext
            where TAction : Enum
            where TAbility : Enum
        {
            List<TAgent> agentsList = new List<TAgent>();

            foreach (TAgent combatAgent in agentsDictionary.Values)
            {
                agentsList.Add(combatAgent);
            }

            return agentsList;
        }*/

        private List<T> ReturnAllDictionaryValuesInAList<T>(Dictionary<uint, T> agentsDictionary)
        {
            List<T> agentsList = new List<T>();

            foreach (T combatAgent in agentsDictionary.Values)
            {
                agentsList.Add(combatAgent);
            }

            return agentsList;
        }
        
        private TReturn ExecuteDelegate<TReturn, TCollection>(TCollection collection, EnemyType enemyType)
            where TCollection : Dictionary<EnemyType, Delegate>
        {
            Delegate del = collection[enemyType];
            
            if (del is Func<TReturn> func)
            {
                return func();
            }
            return default;
        }

        #endregion
    }
}