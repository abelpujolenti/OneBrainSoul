using System;
using System.Collections.Generic;
using ECS.Components.AI.Navigation;
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

        private void Start()
        {
            CombatManager.Instance.AddCombatArea(this, _combatAreaNumber);
        }

        public void AddEnemy(uint enemyId)
        {
            _enemiesInsideArea.Add(enemyId);
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
        }

        private void AddTarget(EntityType entityType, uint targetId)
        {
            _targetEntitiesInsideArea[entityType].Add(targetId);
        }

        public void RemoveTarget(EntityType entityType, uint targetId)
        {
            _targetEntitiesInsideArea[entityType].Remove(targetId);
        }

        public HashSet<uint> GetEntityTypeTargets(EntityType entityType)
        {
            return _targetEntitiesInsideArea[entityType];
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
            
            RemoveTarget(entityType, agentEntity.GetAgentID());
        }
    }
}