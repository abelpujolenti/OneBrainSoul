using System;
using System.Collections.Generic;
using ECS.Entities;
using ECS.Entities.AI;
using UnityEngine;

namespace AI.Combat.Enemy
{
    public class VisionArea : MonoBehaviour
    {
        private EntityType _targetEntities;

        private Action<EntityType, uint> _addAction;
        private Action<EntityType, uint> _removeAction;

        public void Setup(EntityType targetEntities, Action<EntityType, uint> addAction, Action<EntityType, uint> removeAction)
        {
            _targetEntities = targetEntities;
            _addAction = addAction;
            _removeAction = removeAction;
        }

        private void OnTriggerEnter(Collider other)
        {
            AgentEntity agentEntity = other.GetComponent<AgentEntity>();

            if (!agentEntity)
            {
                return;
            }

            EntityType agentEntityType = agentEntity.GetEntityType();

            if ((_targetEntities & agentEntityType) == 0)
            {
                return;
            }

            _addAction(agentEntityType, agentEntity.GetAgentID());
        }

        private void OnTriggerExit(Collider other)
        {
            AgentEntity agentEntity = other.GetComponent<AgentEntity>();

            if (!agentEntity)
            {
                return;
            }

            EntityType agentEntityType = agentEntity.GetEntityType();

            if ((_targetEntities & agentEntityType) == 0)
            {
                return;
            }

            _removeAction(agentEntityType, agentEntity.GetAgentID());
        }
    }
}