using ECS.Components.AI.Navigation;
using Interfaces.AI.Navigation;
using UnityEngine;

namespace ECS.Entities.AI
{
    public abstract class AgentEntity : MonoBehaviour
    {
        private uint _agentId;

        private IPosition _positionComponent;

        private void Start()
        {
            _agentId = (uint)gameObject.GetInstanceID();
            _positionComponent = new VectorComponent(transform.position);
        }

        public uint GetAgentID()
        {
            return _agentId;
        }

        public IPosition GetDestinationComponent()
        {
            return _positionComponent;
        }
    }
}