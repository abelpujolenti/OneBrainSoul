using System;
using ECS.Entities;
using ECS.Entities.AI;
using Managers;
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
            EventsManager.OnAgentDefeated += _removeAction;
        }

        private void OnTriggerEnter(Collider other)
        {
            AgentEntity agentEntity = other.GetComponent<AgentEntity>();

            if (!agentEntity)
            {
                return;
            }

            _addAction(agentEntity.GetEntityType(), agentEntity.GetAgentID());
        }

        private void OnTriggerExit(Collider other)
        {
            AgentEntity agentEntity = other.GetComponent<AgentEntity>();

            if (!agentEntity)
            {
                return;
            }

            _removeAction(agentEntity.GetEntityType(), agentEntity.GetAgentID());
        }

        private void OnDestroy()
        {
            EventsManager.OnAgentDefeated -= _removeAction;
        }
    }
}