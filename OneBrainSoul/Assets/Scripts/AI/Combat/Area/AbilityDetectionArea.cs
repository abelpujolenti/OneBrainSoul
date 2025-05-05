using System;
using ECS.Entities;
using ECS.Entities.AI;
using UnityEngine;

namespace AI.Combat.Area
{
    public class AbilityDetectionArea : MonoBehaviour
    {
        private EntityType _target;
        
        private Action<AgentEntity> _addAction;
        private Action<AgentEntity> _removeAction;
        
        public void Setup(Action<AgentEntity> addAction, Action<AgentEntity> removeAction)
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

            _addAction(agentEntity);
        }

        private void OnTriggerExit(Collider other)
        {
            AgentEntity agentEntity = other.GetComponent<AgentEntity>();

            if (!agentEntity)
            {
                return;
            }

            _removeAction(agentEntity);
        }
    }
}