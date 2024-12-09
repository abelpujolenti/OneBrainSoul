using System;
using System.Collections;
using AI;
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
            _navMeshAgentComponent = new NavMeshAgentComponent(_navMeshAgentSpecs, _navMeshAgent, ownTransform);
            _positionComponent = new VectorComponent(ownTransform.position);
            _rotationSpeed = _navMeshAgentSpecs.rotationSpeed;
            ECSNavigationManager.Instance.AddNavMeshAgentEntity(_agentId, _navMeshAgentComponent, _navMeshAgentSpecs.radius);
        }

        public void ContinueNavigation()
        {
            _navMeshAgentComponent.GetNavMeshAgent().isStopped = false;
        }

        public void StopNavigation()
        {
            _navMeshAgentComponent.GetNavMeshAgent().isStopped = true;
        }

        public void Rotate()
        {
            Vector3 destination = ECSNavigationManager.Instance.GetNavMeshAgentDestination(_agentId).GetPosition();
            
            Transform ownTransform = transform;
            
            Vector3 position = ownTransform.position;
            
            float angleToDestination = Vector3.Angle(ownTransform.forward, destination - position);
                    
            if (angleToDestination > 15f)
            {
                RotateToGivenPosition(destination);
                return;
            }

            StartCoroutine(EnsureAPathExists(() =>
                Vector3.Angle(ownTransform.forward, _navMeshAgent.path.corners[0] - position) > 120f));
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

        public void RotateToNextPathCorner()
        {
            if (_isRotating)
            {
                return;
            }
            
            StopNavigation();

            _isRotating = true;

            StartCoroutine(EnsureAPathExists(()=> true));
        }

        private IEnumerator EnsureAPathExists(Func<bool> action)
        {
            while (_navMeshAgent.path.corners.Length <= 0)
            {
                yield return null;
            }

            if (!action())
            {
                yield break;
            }

            StartCoroutine(RotateToGivenPositionCoroutine(_navMeshAgent.path.corners[0]));
        }

        protected virtual IEnumerator RotateToGivenPositionCoroutine(Vector3 position)
        {
            Transform ownTransform = transform;
            
            Vector3 vectorToNextPathCorner = position - ownTransform.position;
            vectorToNextPathCorner.y = 0;

            while (Vector3.Angle(ownTransform.forward, vectorToNextPathCorner) >= 5f)
            {
                Quaternion rotation = Quaternion.LookRotation(vectorToNextPathCorner);
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
