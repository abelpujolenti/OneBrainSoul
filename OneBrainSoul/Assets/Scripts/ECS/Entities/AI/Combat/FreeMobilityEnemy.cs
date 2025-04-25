using System;
using System.Collections;
using System.Collections.Generic;
using AI.Combat.CombatNavigation;
using AI.Combat.Contexts;
using AI.Combat.Position;
using AI.Combat.ScriptableObjects;
using AI.Combat.Steering;
using AI.Navigation;
using ECS.Components.AI.Navigation;
using Managers;
using UnityEngine;
using UnityEngine.AI;
using Utilities;
using Random = UnityEngine.Random;

namespace ECS.Entities.AI.Combat
{
    public abstract class FreeMobilityEnemy<TContext, TAction> : AIEnemy<FreeMobilityEnemyProperties, TContext, TAction>
        where TContext : FreeMobilityEnemyContext
        where TAction : Enum
    {
        private const string ANIMATOR_VELOCITY_FLOAT = "Velocity";
        
        [SerializeField] protected NavMeshAgentSpecs _navMeshAgentSpecs;

        [SerializeField] protected NavMeshAgent _navMeshAgent;
        
        private NavMeshAgentComponent _navMeshAgentComponent;

        [SerializeField] protected List<Vector3> _pointsToPatrol;
        
        protected AgentSlot _agentSlot;
        
        protected VectorComponent _lastDestination;
        
        protected DirectionWeights[] _raysDirectionAndWeights;
        
        protected float _raysOpeningAngle = 90f;
        protected float _raysDistance = 20f;
        
        protected uint _numberOfVicinityRays = 12;

        protected int _raysTargetsLayerMask;

        private Action _onEnableAction = () => { };

        protected Action _update = () => { };

        protected override void EnemySetup(float radius, FreeMobilityEnemyProperties freeMobilityEnemyProperties, 
            EntityType entityType, EntityType targetEntities)
        {
            base.EnemySetup(radius, freeMobilityEnemyProperties, entityType, targetEntities);
            
            _raysTargetsLayerMask = GameManager.Instance.GetEnemyLayer() + GameManager.Instance.GetGroundLayer() + 1;
            
            _navMeshAgent.speed = _navMeshAgentSpecs.movementSpeed;
            _navMeshAgentComponent = new NavMeshAgentComponent(_navMeshAgentSpecs, _navMeshAgent, GetTransformComponent(), 
                RotateBody, ECSNavigationManager.Instance.SetupNavMeshAgentEntity(GetAgentID()));

            _onEnableAction = () =>
            {
                ECSNavigationManager.Instance.AddNavMeshAgentEntity(GetAgentID(), GetNavMeshAgentComponent(), radius + freeMobilityEnemyProperties.agentsPositionRadius);
            };

            _onEnableAction();

            _update = AILoop;
        }

        #region Navigation
        
        protected void ContinueNavigation()
        {
            _animator.SetFloat(ANIMATOR_VELOCITY_FLOAT, MathUtil.Map(_navMeshAgent.speed, 0, _navMeshAgentSpecs.movementSpeed, 0, 1));
            _navMeshAgent.isStopped = false;
            _context.SetHasStopped(false);
        }

        protected void StopNavigation()
        {
            _animator.SetFloat(ANIMATOR_VELOCITY_FLOAT, 0);
            _navMeshAgent.isStopped = true;
            _context.SetHasStopped(true);
        }

        protected override IEnumerator WaitAnimationExtraTime(float animationExtraTime)
        {
            float tempNavMeshAgentSpeed = _navMeshAgent.speed;

            _navMeshAgent.speed = 0;

            yield return base.WaitAnimationExtraTime(animationExtraTime);

            _navMeshAgent.speed = tempNavMeshAgentSpeed;
        }

        protected void RotateInSitu()
        {
            Vector3 position = transform.position;

            List<Node> nodes = _navMeshAgentComponent.GetAStarPath().path;

            if (nodes.Count == 0)
            {
                Vector3 destination = ECSNavigationManager.Instance.GetNavMeshAgentDestination(GetAgentID()).GetPosition();
                SetDirectionToRotateBody(destination - position);
            }
            else
            {
                SetDirectionToRotateBody(nodes[0].position - position);    
            }

            if (_isRotating)
            {
                return;
            }
            
            //TODO AQUI ROTATION

            _isRotating = true;
            
            BlockFSM();
            
            StopNavigation();
            
            StartCoroutine(RotateToGivenPositionCoroutine());
        }

        private IEnumerator RotateToGivenPositionCoroutine()
        {
            while (Vector3.Dot(transform.forward, GetDirectionToRotateBody()) < 0.99f)
            {
                RotateBody();
                yield return null;
            }
            
            _isRotating = false;
            
            UnblockFSM();
            
            ContinueNavigation();
        }

        private bool SampleSurfacePosition(Vector3 position, out Vector3 sampledPosition)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(position, out hit, 2f, NavMesh.AllAreas))
            {
                sampledPosition = hit.position;
                return true;
            }

            sampledPosition = Vector3.zero;
            return false;
        }

        protected override void GoToArea(Vector3 estimatedPosition)
        {
            SetDestination(new VectorComponent(estimatedPosition));
        }

        protected override void OnEndInvestigation()
        {
            base.OnEndInvestigation();
            //TODO FREE MOBILITY ENEMY CHOOSE NEW POINT TO PATROL
        }

        public NavMeshAgentComponent GetNavMeshAgentComponent()
        {
            return _navMeshAgentComponent;
        }

        public void SetDestination(TransformComponent transformComponent)
        {
            _navMeshAgentComponent.GetAStarPath().OnSetNewDestination();
            _lastDestination = null;
            ECSNavigationManager.Instance.UpdateNavMeshAgentDestination(GetAgentID(), transformComponent);
        }

        public void SetDestination(VectorComponent vectorComponent)
        {
            _navMeshAgentComponent.GetAStarPath().OnSetNewDestination();
            _lastDestination = vectorComponent;
            ECSNavigationManager.Instance.UpdateNavMeshAgentDestination(GetAgentID(), _lastDestination);
        }

        #endregion

        #region FSM

        private void Update()
        {
            _update();
        }

        protected abstract void AILoop();
        
        protected void GoToDestination()
        {
            ShowDebugMessages("Triface " + GetAgentID() + " Going To Destination");
            
            ContinueNavigation();
        }

        protected void Patrol()
        {
            ShowDebugMessages("Triface " + GetAgentID() + " Patrolling");
            
            //TODO TRIFACE PATROL, TODO POINTS OF PATROL
        }

        protected override void InvestigateArea()
        {
            SetDirectionToRotateBody(transform.forward);
            _timeInvestigating = Random.Range(_minimumTimeInvestigatingArea, _maximumTimeInvestigatingArea);
            
            base.InvestigateArea();
        }

        protected void GoToClosestSightedTarget()
        {
            ShowDebugMessages("Triface " + GetAgentID() + " Going To Closest Sighted Target");
            
            SetDestination(CombatManager.Instance.ReturnClosestAgentEntity(transform.position, _targetsSightedInsideCombatArea)
                .GetTransformComponent());
        }

        #endregion

        public override void OnReceiveSlow(uint slowID, uint slowPercent, Vector3 sourcePosition)
        {
            base.OnReceiveSlow(slowID, slowPercent, sourcePosition);
            
            //TODO SLOW FREE MOBILITY
        }

        protected virtual void OnEnable()
        {
            _onEnableAction();
        }

        protected virtual void OnDisable()
        {
            RemoveNavMeshEntity();
        }

        protected override void PreDeath()
        {
            base.PreDeath();
            _update = () => { };
            RemoveNavMeshEntity();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            RemoveNavMeshEntity();
        }

        private void RemoveNavMeshEntity()
        {
            ECSNavigationManager.Instance.RemoveNavMeshAgentEntity(GetAgentID());
        }

        protected override void OnDrawGizmos()
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
            return new Vector3(position.x, position.y + 5, position.z);
        }
    }
}