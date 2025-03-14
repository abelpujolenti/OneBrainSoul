using System;
using ECS.Entities;
using ECS.Entities.AI;
using UnityEngine;

namespace AI.Combat.Area
{
    public class AbilityDetectionArea : MonoBehaviour
    {
        private EntityType _target;
        
        private Action<uint> _addAction;
        private Action<uint> _removeAction;
        
        public void Setup(EntityType target, Action<uint> addAction, Action<uint> removeAction)
        {
            _target = target;
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

            if ((_target & agentEntity.GetEntityType()) == 0)
            {
                return;
            }

            _addAction(agentEntity.GetAgentID());
        }

        private void OnTriggerExit(Collider other)
        {
            AgentEntity agentEntity = other.GetComponent<AgentEntity>();

            if (!agentEntity)
            {
                return;
            }

            if ((_target & agentEntity.GetEntityType()) == 0)
            {
                return;
            }

            _removeAction(agentEntity.GetAgentID());
        }
    }
}