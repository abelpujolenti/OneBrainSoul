using System.Collections;
using System.Collections.Generic;
using AI.Combat.CombatNavigation;
using AI.Navigation;
using ECS.Components.AI.Navigation;
using Interfaces.AI.Navigation;
using Managers;
using UnityEngine;
using UnityEngine.AI;

namespace ECS.Entities.AI.Navigation
{
    public class NavMeshAgentEntity : MonoBehaviour
    {
        [SerializeField] protected NavMeshAgentSpecs _navMeshAgentSpecs;

        [SerializeField] protected NavMeshAgent _navMeshAgent;

        private uint _agentId;

        private NavMeshAgentComponent _navMeshAgentComponent;

        private IPosition _positionComponent;

        protected float _rotationSpeed;

        protected bool _isRotating;

        protected void Setup()
        {
            _agentId = (uint)gameObject.GetInstanceID();
            Transform ownTransform = transform;
            _navMeshAgent.speed = _navMeshAgentSpecs.speed;
            _navMeshAgentComponent = new NavMeshAgentComponent(_navMeshAgentSpecs, _navMeshAgent, ownTransform);
            _positionComponent = new VectorComponent(ownTransform.position);
            _rotationSpeed = _navMeshAgentSpecs.rotationSpeed;
        }

        protected void ContinueNavigation()
        {
            _navMeshAgentComponent.GetNavMeshAgent().isStopped = false;
        }

        protected void StopNavigation()
        {
            _navMeshAgentComponent.GetNavMeshAgent().isStopped = true;
        }

        public void Rotate()
        {
            Vector3 destination = ECSNavigationManager.Instance.GetNavMeshAgentDestination(_agentId).GetPosition();

            List<Node> nodes = _navMeshAgentComponent.GetAStarPath().path;
                    
            if (nodes.Count == 0)
            {
                RotateToGivenPosition(destination);
                return;
            }
            
            RotateToGivenPosition(nodes[0].position);
        }

        private void RotateToGivenPosition(Vector3 position)
        {
            if (_isRotating)
            {
                return;
            }
            
            StopNavigation();

            _isRotating = true;
            
            StartCoroutine(RotateToGivenPositionCoroutine(position));
        }

        protected virtual IEnumerator RotateToGivenPositionCoroutine(Vector3 position)
        {
            Transform ownTransform = transform;
            
            Vector3 vectorToPosition = position - ownTransform.position;
            vectorToPosition.y = 0;

            while (Vector3.Angle(ownTransform.forward, vectorToPosition) >= 5f)
            {
                Quaternion rotation = Quaternion.LookRotation(vectorToPosition);
                ownTransform.rotation = Quaternion.Slerp(ownTransform.rotation, rotation, _rotationSpeed * Time.deltaTime);
                yield return null;
            }
            
            _isRotating = false;
            
            ContinueNavigation();
        }

        public uint GetAgentID()
        {
            return _agentId;
        }

        public NavMeshAgentComponent GetNavMeshAgentComponent()
        {
            return _navMeshAgentComponent;
        }

        public IPosition GetDestinationComponent()
        {
            return _positionComponent;
        }
    }
}
