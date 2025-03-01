using System.Collections.Generic;
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

        private Dictionary<EntityType, HashSet<uint>> _targetEntitiesInsideArea =
            new Dictionary<EntityType, HashSet<uint>>();

        private Dictionary<EntityType, HashSet<uint>> _targetEntitiesSightedInsideArea =
            new Dictionary<EntityType, HashSet<uint>>();

        private uint _playerId;

        public void SetPlayerId(uint playerId)
        {
            _playerId = playerId;
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

            _targetEntitiesInsideArea[entityType].Add(enemyId);
        }
        
        public void RemoveEnemy(uint enemyId)
        {
            _enemiesInsideArea.Remove(enemyId);
        }

        public bool IsAreaEmpty()
        {
            return _enemiesInsideArea.Count == 0;
        }

        public void AddEntityType(EntityType entityType)
        {
            if (_targetEntitiesInsideArea.ContainsKey(entityType))
            {
                return;
            }
            
            _targetEntitiesInsideArea.Add(entityType, new HashSet<uint>());
            _targetEntitiesSightedInsideArea.Add(entityType, new HashSet<uint>());
        }

        private void AddTarget(EntityType entityType, uint targetId)
        {
            _targetEntitiesInsideArea[entityType].Add(targetId);
        }

        public void RemoveTarget(EntityType entityType, uint targetId)
        {
            _targetEntitiesInsideArea[entityType].Remove(targetId);
        }

        public void AddSightedTarget(EntityType entityType, uint targetId)
        {
            if (_targetEntitiesSightedInsideArea[entityType].Contains(targetId))
            {
                return;
            }
            
            _targetEntitiesSightedInsideArea[entityType].Add(targetId);

            if (targetId != _playerId)
            {
                return;
            }
            
            CombatManager.Instance.OnPlayerDetection();
        }

        private void RemoveSightedTarget(EntityType entityType, uint targetId)
        {
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
    }
}