using System;
using System.Collections;
using System.Collections.Generic;
using AI.Combat.CombatNavigation;
using AI.Combat.Contexts;
using AI.Combat.ScriptableObjects;
using AI.Combat.Steering;
using AI.Navigation;
using ECS.Components.AI.Navigation;
using Managers;
using UnityEngine;
using UnityEngine.AI;
using Utilities;

namespace ECS.Entities.AI.Combat
{
    public abstract class FreeMobilityEnemy<TContext, TAction> : AIEnemy<TContext, TAction>
        where TContext : AIEnemyContext
        where TAction : Enum
    {
        [SerializeField] protected NavMeshAgentSpecs _navMeshAgentSpecs;

        [SerializeField] protected NavMeshAgent _navMeshAgent;
        
        private NavMeshAgentComponent _navMeshAgentComponent;
        
        protected VectorComponent _lastDestination;
        
        protected DirectionWeights[] _raysDirectionAndWeights;
        
        protected float _rotationSpeed;
        protected float _raysOpeningAngle = 90f;
        protected float _raysDistance = 20f;
        
        protected uint _numberOfVicinityRays = 12;

        protected int _raysTargetsLayerMask;
        
        protected bool _isRotating;

        protected override void EnemySetup(float radius, AIEnemySpecs aiEnemySpecs)
        {
            base.EnemySetup(radius, aiEnemySpecs);
            
            _raysTargetsLayerMask = GameManager.Instance.GetEnemyLayer() + GameManager.Instance.GetGroundLayer() + 1;
            
            _navMeshAgent.speed = _navMeshAgentSpecs.movementSpeed;
            _navMeshAgentComponent = new NavMeshAgentComponent(_navMeshAgentSpecs, _navMeshAgent, GetTransformComponent());
            _rotationSpeed = _navMeshAgentSpecs.rotationSpeed;
            
            ECSNavigationManager.Instance.AddNavMeshAgentEntity(GetAgentID(), GetNavMeshAgentComponent(), radius);
        }

        #region Steering

        protected void SetRaysDirections()
        {
            _raysDirectionAndWeights = new DirectionWeights[_numberOfVicinityRays];
            
            float angle = -(_raysOpeningAngle / 2);
            float angleStep = _raysOpeningAngle / _numberOfVicinityRays;

            Transform ownTransform = transform;

            for (int i = 0; i < _numberOfVicinityRays; i++)
            {
                Vector3 direction = Quaternion.Euler(0, angle, 0) * ownTransform.forward;
                _raysDirectionAndWeights[i].direction = direction.normalized;
                angle += angleStep;
            }
        }

        protected void LaunchRaycasts()
        {
            SetRaysDirections();
            
            Vector3 position = transform.position;

            RaycastHit hit;
            
            for (int i = 0; i < _numberOfVicinityRays; i++)
            {
                if (Physics.Raycast(position, _raysDirectionAndWeights[i].direction, out hit, _raysDistance, _raysTargetsLayerMask))
                {
                    _raysDirectionAndWeights[i].weight = MathUtil.Map(hit.distance, 0, 1, _raysDistance, 0);
                    //Debug.Log(hit.collider.name);
                    continue;
                }

                _raysDirectionAndWeights[i].weight = 0;
            }
        }

        #endregion

        #region Navigation
        
        protected void ContinueNavigation()
        {
            _navMeshAgent.isStopped = false;
        }

        protected void StopNavigation()
        {
            _navMeshAgent.isStopped = true;
        }

        protected void Rotate()
        {
            Vector3 destination = ECSNavigationManager.Instance.GetNavMeshAgentDestination(GetAgentID()).GetPosition();

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

        private IEnumerator RotateToGivenPositionCoroutine(Vector3 position)
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

        public NavMeshAgentComponent GetNavMeshAgentComponent()
        {
            return _navMeshAgentComponent;
        }

        public void SetDestination(TransformComponent transformComponent)
        {
            _lastDestination = null;
            ECSNavigationManager.Instance.UpdateNavMeshAgentDestination(GetAgentID(), transformComponent);
        }

        public void SetDestination(VectorComponent vectorComponent)
        {
            _lastDestination = vectorComponent;
            ECSNavigationManager.Instance.UpdateNavMeshAgentDestination(GetAgentID(), _lastDestination);
        }

        #endregion

        protected override void CastingAnAbility()
        {
            base.CastingAnAbility();
            ContinueNavigation();
        }

        protected override void NotCastingAnAbility()
        {
            base.NotCastingAnAbility();
            StopNavigation();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ECSNavigationManager.Instance.RemoveNavMeshAgentEntity(GetAgentID(), true);
        }

        private void OnDrawGizmos()
        {
            if (ECSNavigationManager.Instance == null)
            {
                return;
            }
            
            /*Vector3 position = transform.position;

            Gizmos.color = Color.green;

            foreach (DirectionWeights directionAndWeight in _raysDirectionAndWeights)
            {
                Gizmos.DrawRay(position, directionAndWeight.direction * _raysDistance);
            }*/

            Vector3[] corners = GetCorners();

            if (corners.Length == 0)
            {
                return;
            }
            
            Gizmos.color = Color.blue;

            for (int i = 0; i < corners.Length - 1; i++)
            {
                Gizmos.DrawSphere(Up(corners[i]), 0.2f);
                
                Gizmos.DrawLine(Up(corners[i]), Up(corners[i + 1]));
            }
            
            Gizmos.DrawSphere(Up(corners[^1]), 0.2f);
        }

        private Vector3[] GetCorners()
        {
            List<Vector3> corners = new List<Vector3>();

            foreach (Node node in GetNavMeshAgentComponent().GetAStarPath().path)
            {
                corners.Add(node.position);
            }

            return corners.ToArray();
        }

        private Vector3 Up(Vector3 position)
        {
            return new Vector3(position.x, position.y + 0, position.z);
        }
    }
}