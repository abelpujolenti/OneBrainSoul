using System;
using System.Collections.Generic;
using AI.Combat.Area;
using ECS.Entities;
using ECS.Entities.AI;
using ECS.Entities.AI.Combat;
using Player;
using UnityEngine;

namespace Managers
{
    public class CombatManager : MonoBehaviour
    {
        private static CombatManager _instance;

        public static CombatManager Instance => _instance;

        private Dictionary<uint, Func<AgentEntity>> _returnAgent = new Dictionary<uint, Func<AgentEntity>>();

        private PlayerCharacter _playerCharacter;
        private Dictionary<uint, Triface> _trifaces = new Dictionary<uint, Triface>();
        private Dictionary<uint, LongArms> _longArms = new Dictionary<uint, LongArms>();
        private Dictionary<uint, LongArmsBase> _longArmsBases = new Dictionary<uint, LongArmsBase>();
        private Dictionary<uint, Sendatu> _sendatus = new Dictionary<uint, Sendatu>();

        private HashSet<uint> _longArmsBasesFreeId = new HashSet<uint>();

        private Dictionary<uint, CombatArea> _combatAreas = new Dictionary<uint, CombatArea>();
        
        private readonly Dictionary<EntityType, Delegate> _returnDictionaryOfTheSameType = new Dictionary<EntityType, Delegate>
        {
            {
                EntityType.TRIFACE, new Func<Dictionary<uint, Triface>>(() => _instance._trifaces)
            },
            {
                EntityType.LONG_ARMS, new Func<Dictionary<uint, LongArms>>(() => _instance._longArms)
            },
            {
                EntityType.LONG_ARMS_BASE, new Func<Dictionary<uint, LongArmsBase>>(() => _instance._longArmsBases)
            },
            {
                EntityType.SENDATU, new Func<Dictionary<uint, Sendatu>>(() => _instance._sendatus)
            }
        };

        private readonly Dictionary<EntityType, Delegate> _returnTheSameAgentsType = new Dictionary<EntityType, Delegate>
        {
            { 
                EntityType.TRIFACE, new Func<List<Triface>>(() => 
                _instance.ReturnAllDictionaryValuesInAList<Triface>(_instance._trifaces)) 
            },
            { 
                EntityType.LONG_ARMS, new Func<List<LongArms>>(() => 
                _instance.ReturnAllDictionaryValuesInAList<LongArms>(_instance._longArms)) 
            },
            { 
                EntityType.LONG_ARMS_BASE, new Func<List<LongArmsBase>>(() => 
                _instance.ReturnAllDictionaryValuesInAList<LongArmsBase>(_instance._longArmsBases)) 
            },
            { 
                EntityType.SENDATU, new Func<List<Sendatu>>(() => 
                _instance.ReturnAllDictionaryValuesInAList<Sendatu>(_instance._sendatus)) 
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
        
        #region Add Combat Agent

        public void AddPlayer(PlayerCharacter playerCharacter)
        {
            _playerCharacter = playerCharacter;
            
            _returnAgent.Add(playerCharacter.GetAgentID(), () => _playerCharacter);
        }

        public void AddEnemy(Triface triface)
        {
            uint agentID = triface.GetAgentID();
            
            _trifaces.Add(agentID, triface);
            _returnAgent.Add(agentID, () => _trifaces[agentID]);

            AddEnemyToAreaNumber(triface.GetAreaNumber(), agentID, triface.GetTarget());
        }

        public void AddEnemy(LongArms longArms)
        {
            uint agentID = longArms.GetAgentID();
            
            _longArms.Add(agentID, longArms);
            _returnAgent.Add(agentID, () => _longArms[agentID]);

            for (int i = 0; i < _longArmsBasesFreeId.Count; i++)
            {
                longArms.IncrementLongArmsFreeBases();
            }

            AddEnemyToAreaNumber(longArms.GetAreaNumber(), agentID, longArms.GetTarget());
        }

        public void AddEnemy(LongArmsBase longArmsBase)
        {
            uint agentID = longArmsBase.GetAgentID();
            
            _longArmsBases.Add(agentID, longArmsBase);
            _returnAgent.Add(agentID, () => _longArmsBases[agentID]);
            IncrementLongArmsBasesFree(agentID);
        }

        public void AddEnemy(Sendatu sendatu)
        {
            uint agentID = (uint)sendatu.GetInstanceID();
            
            _sendatus.Add(agentID, sendatu);
            _returnAgent.Add(agentID, () => _sendatus[agentID]);

            AddEnemyToAreaNumber(sendatu.GetAreaNumber(), agentID, sendatu.GetTarget());
        }

        #endregion

        #region Combat Areas

        public void AddCombatArea(CombatArea combatArea, uint areaNumber)
        {
            _combatAreas.Add(areaNumber, combatArea);
        }

        private void AddEnemyToAreaNumber(uint areaNumber, uint enemyId, EntityType target)
        {
            CombatArea combatArea = _combatAreas[areaNumber];
            
            combatArea.AddEnemy(enemyId);

            for (EntityType i = 0; i < EntityType.ENUM_SIZE; i++)
            {
                if ((target & i) == 0)
                {
                    continue;
                }
                
                combatArea.AddEntityType(i);
            }
        }

        #endregion

        #region UBS

        public HashSet<uint> ReturnVisibleTargets(EntityType target, Vector3 position, 
            Dictionary<EntityType, HashSet<uint>> targetsInsideVisionArea, uint areaNumber)
        {
            HashSet<uint> visibleTargets = new HashSet<uint>();

            for (EntityType i = 0; i < EntityType.ENUM_SIZE; i++)
            {
                if ((target & i) == 0 || !targetsInsideVisionArea.ContainsKey(i))
                {
                    continue;
                }
                
                HashSet<uint> targetsInsideCombatArea = _combatAreas[areaNumber].GetEntityTypeTargets(i);

                if (targetsInsideCombatArea.Count == 0)
                {
                    continue;
                }
                    
                foreach (uint targetId in targetsInsideVisionArea[i])
                {
                    if (!targetsInsideCombatArea.Contains(targetId))
                    {
                        continue;
                    }
                    
                    AgentEntity agentEntity = _returnAgent[targetId]();

                    if (!CanSeeEntity(agentEntity.GetTransformComponent().GetPosition(), agentEntity.GetRadius(), position))
                    {
                        continue;
                    }
                    visibleTargets.Add(agentEntity.GetAgentID());
                }
            }

            return visibleTargets;
        }

        private bool CanSeeEntity(Vector3 targetPosition, float targetRadius, Vector3 position)
        {
            Vector3 vectorToTarget = (targetPosition - position).normalized;
            float distanceToTarget = (targetPosition - position).magnitude - targetRadius;

            return !Physics.Raycast(position, vectorToTarget, distanceToTarget, 
                GameManager.Instance.GetGroundLayer());
        }

        public void AddFreeLongArmsBaseId(uint longArmsBaseId)
        {
            _longArmsBasesFreeId.Add(longArmsBaseId);
        }

        private void IncrementLongArmsBasesFree(uint longArmsBaseId)
        {
            AddFreeLongArmsBaseId(longArmsBaseId);
            
            List<LongArms> allLongArms = ReturnAllLongArms();

            foreach (LongArms longArms in allLongArms)
            {
                longArms.IncrementLongArmsFreeBases();
            }
        }

        public void RemoveFreeLongArmsBaseId(uint longArmsBaseId)
        {
            _longArmsBasesFreeId.Remove(longArmsBaseId);
        }

        private void DecrementLongArmsBasesFree(uint longArmsBaseId)
        {
            RemoveFreeLongArmsBaseId(longArmsBaseId);
            
            List<LongArms> allLongArms = ReturnAllLongArms();

            foreach (LongArms longArms in allLongArms)
            {
                longArms.DecrementLongArmsFreeBases();
            }
        }

        #endregion

        #region Requests

        public AgentEntity ReturnAgentEntity(uint agentId)
        {
            return _returnAgent[agentId]();
        }

        public PlayerCharacter ReturnPlayer()
        {
            return _playerCharacter;
        }

        private List<Triface> ReturnAllTrifaces()
        {
            return ExecuteDelegate<List<Triface>, Dictionary<EntityType, Delegate>>(_returnTheSameAgentsType,
                EntityType.TRIFACE);
        }

        private List<LongArms> ReturnAllLongArms()
        {
            return ExecuteDelegate<List<LongArms>, Dictionary<EntityType, Delegate>>(_returnTheSameAgentsType,
                EntityType.LONG_ARMS);
        }

        private List<LongArmsBase> ReturnAllLongArmsBases()
        {
            return ExecuteDelegate<List<LongArmsBase>, Dictionary<EntityType, Delegate>>(_returnTheSameAgentsType,
                EntityType.LONG_ARMS_BASE);
        }

        private List<Sendatu> RequestAllSendatus()
        {
            return ExecuteDelegate<List<Sendatu>, Dictionary<EntityType, Delegate>>(_returnTheSameAgentsType,
                EntityType.SENDATU);
        }

        public List<AgentEntity> ReturnAllEnemies()
        {
            List<AgentEntity> enemies = new List<AgentEntity>();

            enemies.AddRange(ReturnAllTrifaces());
            
            enemies.AddRange(ReturnAllLongArms());
            
            enemies.AddRange(ReturnAllLongArmsBases());
            
            enemies.AddRange(RequestAllSendatus());

            return enemies;
        }

        public AgentEntity ReturnClosestAgentEntity(Vector3 position, HashSet<uint> targetsId)
        {
            return ReturnClosestTargetAgent(position, targetsId, targetId => _returnAgent[targetId]());
        }

        public LongArmsBase ReturnClosestLongArmsBase(Vector3 position, HashSet<uint> longArmsBasesId)
        {
            return ReturnClosestTargetAgent(position, longArmsBasesId, longArmsBaseId => _longArmsBases[longArmsBaseId]);
        }

        private T ReturnClosestTargetAgent<T>(Vector3 position, HashSet<uint> targetsId, Func<uint, T> returnFunc)
            where T : AgentEntity
        {
            T closestTarget = default;
            T currentAgentEntity;

            Vector3 targetPosition;

            float closestDistance = Mathf.Infinity;
            float currentDistance;
            
            foreach (uint targetId in targetsId)
            {
                currentAgentEntity = returnFunc(targetId);

                targetPosition = currentAgentEntity.GetTransformComponent().GetPosition();
                targetPosition.y -= currentAgentEntity.GetHeight() / 2;
                
                currentDistance = (position - targetPosition).sqrMagnitude;

                if (closestDistance < currentDistance)
                {
                    continue;
                }

                closestTarget = currentAgentEntity;
                closestDistance = currentDistance;
            }

            return closestTarget;
        }

        public float ReturnDistanceToTarget(Vector3 position, uint targetId)
        {
            return (_returnAgent[targetId]().GetTransformComponent().GetPosition() - position).magnitude;
        }

        #region Long Arms

        public void RequestFleeToAnotherLongArmsBase(LongArms longArms)
        {
            LongArmsBase longArmsBase = ReturnClosestLongArmsBase(longArms.transform.position, _longArmsBasesFreeId);
            
            longArms.CallOnFleeAction();
            
            longArmsBase.SetLongArms(longArms);
        }

        #endregion

        #endregion

        #region Combat Agents Events

        public void OnEnemyDefeated(Triface triface, uint areaNumber)
        {
            uint trifaceId = triface.GetAgentID();
            
            _combatAreas[areaNumber].RemoveEnemy(trifaceId);

            if (_combatAreas[areaNumber].IsAreaEmpty())
            {
                _combatAreas.Remove(areaNumber);
            }
            
            _returnAgent.Remove(trifaceId);
            _trifaces.Remove(trifaceId);
        }

        public void OnEnemyDefeated(LongArms longArms, uint areaNumber)
        {
            uint longArmsId = longArms.GetAgentID();
            
            _combatAreas[areaNumber].RemoveEnemy(longArmsId);

            if (_combatAreas[areaNumber].IsAreaEmpty())
            {
                _combatAreas.Remove(areaNumber);
            }
            
            _returnAgent.Remove(longArmsId);
            _longArms.Remove(longArmsId);
            IncrementLongArmsBasesFree(longArms.CallLongArmsBaseIdFunc());
        }

        public void OnEnemyDefeated(LongArmsBase longArmsBase)
        {
            uint longArmsBaseId = longArmsBase.GetAgentID();
            
            _returnAgent.Remove(longArmsBaseId);
            _longArms.Remove(longArmsBaseId);
            DecrementLongArmsBasesFree(longArmsBase.GetAgentID());
        }

        public void OnEnemyDefeated(Sendatu sendatu, uint areaNumber)
        {
            uint sendatuId = sendatu.GetAgentID();
            
            _combatAreas[areaNumber].RemoveEnemy(sendatuId);

            if (_combatAreas[areaNumber].IsAreaEmpty())
            {
                _combatAreas.Remove(areaNumber);
            }
            _returnAgent.Remove(sendatuId);
            _longArms.Remove(sendatuId);
        }

        public void HealPlayer()
        {
            _playerCharacter.OnReceiveHeal(GameManager.Instance.GetHealPerDeath(), _playerCharacter.transform.position);
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

        private Dictionary<uint, T> ReturnAgentTypeDictionary<T>(EntityType enemyType)
        {
            return ExecuteDelegate<Dictionary<uint, T>, Dictionary<EntityType, Delegate>>
                (_returnDictionaryOfTheSameType, enemyType);
        }

        private List<T> ReturnAllDictionaryValuesInAList<T>(Dictionary<uint, T> agentsDictionary)
        {
            List<T> agentsList = new List<T>();

            foreach (T combatAgent in agentsDictionary.Values)
            {
                agentsList.Add(combatAgent);
            }

            return agentsList;
        }
        
        private TReturn ExecuteDelegate<TReturn, TCollection>(TCollection collection, EntityType entityType)
            where TCollection : Dictionary<EntityType, Delegate>
        {
            Delegate del = collection[entityType];
            
            if (del is Func<TReturn> func)
            {
                return func();
            }
            return default;
        }

        #endregion
    }
}