using System;
using ECS.Entities;
using ECS.Entities.AI;
using UnityEngine;

namespace AI.Combat.Area
{
    public class VisionArea : MonoBehaviour
    {
        private Action<EntityType, uint> _addAction;
        private Action<EntityType, uint> _removeAction;

        public void Setup(Action<EntityType, uint> addAction, Action<EntityType, uint> removeAction)
        {
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

            _removeAction(agentEntityType, agentEntity.GetAgentID());
        }
    }
}