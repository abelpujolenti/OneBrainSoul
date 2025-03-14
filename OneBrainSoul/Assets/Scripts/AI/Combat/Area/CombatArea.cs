using System.Collections.Generic;
using System.Linq;
using ECS.Entities;
using ECS.Entities.AI;
using Managers;
using UnityEngine;

namespace AI.Combat.Area
{
    public class CombatArea : MonoBehaviour
    {
        [SerializeField] private uint _combatAreaNumber;
        
        private HashSet<uint> _enemiesInsideArea = new HashSet<uint>();

        private Dictionary<EntityType, HashSet<uint>> _agentsTargets = new Dictionary<EntityType, HashSet<uint>>();

        private Dictionary<EntityType, HashSet<uint>> _targetEntitiesInsideArea =
            new Dictionary<EntityType, HashSet<uint>>();

        private Dictionary<EntityType, HashSet<uint>> _targetEntitiesSightedInsideArea =
            new Dictionary<EntityType, HashSet<uint>>();

        private uint _playerId;

        public bool _debug;

        private void Awake()
        {
            CombatManager.Instance.AddCombatArea(this, _combatAreaNumber);
        }

        private void Start()
        {
            EventsManager.OnAgentDefeated += RemoveTarget;
            EventsManager.OnAgentDefeated += RemoveSightedTarget;
        }

        public void SetPlayerId(uint playerId)
        {
            _playerId = playerId;
        }

        public bool HasPlayerInside()
        {
            return _targetEntitiesInsideArea.ContainsKey(EntityType.PLAYER) &&
                   _targetEntitiesInsideArea[EntityType.PLAYER].Contains(_playerId);
        }

        public uint GetCombatAreaNumber()
        {
            return _combatAreaNumber;
        }

        public void AddEnemy(uint enemyId, EntityType entityType)
        {
            _enemiesInsideArea.Add(enemyId);

            if (!_targetEntitiesInsideArea.ContainsKey(entityType))
            {
                return;
            }

            AddTarget(entityType, enemyId);
            AddSightedTarget(entityType, enemyId);
        }
        
        public void RemoveEnemy(uint enemyId, EntityType entityType)
        {
            _enemiesInsideArea.Remove(enemyId);

            RemoveTarget(entityType, enemyId);

            List<EntityType> keysToRemove = new List<EntityType>();

            foreach (EntityType targetEntityType in _agentsTargets.Keys)
            {
                if (!_agentsTargets[targetEntityType].Remove(enemyId))
                {
                    continue;
                }

                if (_agentsTargets[targetEntityType].Count != 0)
                {
                    continue;
                }
                
                keysToRemove.Add(targetEntityType);
            }

            foreach (EntityType keyToRemove in keysToRemove)
            {
                _agentsTargets.Remove(keyToRemove);
                _targetEntitiesInsideArea.Remove(keyToRemove);
                _targetEntitiesSightedInsideArea.Remove(keyToRemove);
            }
        }

        public bool IsAreaEmpty()
        {
            return _enemiesInsideArea.Count == 0;
        }

        public void AddTargetEntityType(EntityType newTargetEntityType, uint enemyId)
        {
            if (_agentsTargets.ContainsKey(newTargetEntityType))
            {
                _agentsTargets[newTargetEntityType].Add(enemyId);
            }
            else
            {
                _agentsTargets.Add(newTargetEntityType, new HashSet<uint>
                {
                    enemyId
                });
            }
            
            if (_targetEntitiesInsideArea.ContainsKey(newTargetEntityType))
            {
                return;   
            }
            
            _targetEntitiesInsideArea.Add(newTargetEntityType, new HashSet<uint>());
            _targetEntitiesSightedInsideArea.Add(newTargetEntityType, new HashSet<uint>());

            List<AgentEntity> agentEntities = CombatManager.Instance.ReturnAgentEntities(_enemiesInsideArea);
            
            foreach (AgentEntity agentEntity in agentEntities)
            {
                if (newTargetEntityType != agentEntity.GetEntityType())
                {
                    continue;
                }

                uint agentId = agentEntity.GetAgentID();

                if (_targetEntitiesInsideArea[newTargetEntityType].Contains(agentId))
                {
                    continue;
                }

                AddTarget(newTargetEntityType, agentId);
                AddSightedTarget(newTargetEntityType, agentId);
            }
        }

        private void AddTarget(EntityType entityType, uint targetId)
        {
            _targetEntitiesInsideArea[entityType].Add(targetId);
        }

        private void RemoveTarget(EntityType entityType, uint targetId)
        {
            if (!_targetEntitiesInsideArea.ContainsKey(entityType))
            {
                return;
            }
            
            _targetEntitiesInsideArea[entityType].Remove(targetId);
        }

        public void AddSightedTarget(EntityType entityType, uint targetId)
        {
            _targetEntitiesSightedInsideArea[entityType].Add(targetId);

            if (targetId != _playerId)
            {
                return;
            }
            
            CombatManager.Instance.OnPlayerDetection();
        }

        private void RemoveSightedTarget(EntityType entityType, uint targetId)
        {
            if (!_targetEntitiesSightedInsideArea.ContainsKey(entityType))
            {
                return;
            }
            
            _targetEntitiesSightedInsideArea[entityType].Remove(targetId);

            if (targetId != _playerId)
            {
                return;
            }
            
            CombatManager.Instance.OnLosePlayerDetection();
        }

        public HashSet<uint> GetEnemiesInside()
        {
            return _enemiesInsideArea;
        }

        public HashSet<uint> GetEntityTypeTargets(EntityType entityType)
        {
            return _targetEntitiesInsideArea[entityType];
        }

        public HashSet<uint> GetEntityTypeTargetsSighted(EntityType entityType)
        {
            return _targetEntitiesSightedInsideArea[entityType];
        }

        private void OnTriggerEnter(Collider other)
        {
            AgentEntity agentEntity = other.GetComponent<AgentEntity>();

            if (!agentEntity)
            {
                return;
            }

            EntityType entityType = agentEntity.GetEntityType();

            if (!_targetEntitiesInsideArea.ContainsKey(entityType))
            {
                return;
            }
            
            AddTarget(entityType, agentEntity.GetAgentID());
        }

        private void OnTriggerExit(Collider other)
        {
            AgentEntity agentEntity = other.GetComponent<AgentEntity>();

            if (!agentEntity)
            {
                return;
            }

            EntityType entityType = agentEntity.GetEntityType();

            if (!_targetEntitiesInsideArea.ContainsKey(entityType))
            {
                return;
            }

            uint agentId = agentEntity.GetAgentID();
            
            RemoveTarget(entityType, agentId);
            
            RemoveSightedTarget(entityType, agentId);
        }

        private void OnDestroy()
        {
            EventsManager.OnAgentDefeated -= RemoveTarget;
            EventsManager.OnAgentDefeated -= RemoveSightedTarget;
            
            foreach (EntityType entityType in _targetEntitiesInsideArea.Keys)
            {
                uint[] agentsId = _targetEntitiesInsideArea[entityType].ToArray();
                foreach (uint agentId in agentsId)
                {
                    RemoveTarget(entityType, agentId);
                    RemoveSightedTarget(entityType, agentId);
                }
            }
        }
    }
}