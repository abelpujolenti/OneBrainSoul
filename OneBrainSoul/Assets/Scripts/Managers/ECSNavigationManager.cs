using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using AI.Combat.CombatNavigation;
using AI.Navigation;
using ECS.Components.AI.Navigation;
using ECS.Systems.AI.Navigation;
using Interfaces.AI.Navigation;
using Threads;
using UnityEngine;
using UnityEngine.AI;

namespace Managers
{
    public class ECSNavigationManager : MonoBehaviour
    {
        private static ECSNavigationManager _instance;

        public static ECSNavigationManager Instance => _instance;

        [SerializeField] private float _scalingFactor;

        [SerializeField] private float _timeToCalculatePath;
        
        private Dictionary<NavMeshAgentComponent, AStarPath> _navMeshAgentDestinations = 
            new Dictionary<NavMeshAgentComponent, AStarPath>();

        private List<Thread> _pathfindingThreads = new List<Thread>();

        private MainThreadQueue _mainThreadQueue = new MainThreadQueue();

        private UpdateAgentDestinationSystem _updateAgentDestinationSystem = new UpdateAgentDestinationSystem();

        private NavMeshGraph _navMeshGraph = new NavMeshGraph();

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;

                _navMeshGraph.BuildGraph(NavMesh.CalculateTriangulation(), _scalingFactor);
                
                DontDestroyOnLoad(gameObject);
                
                return;
            }
            
            Destroy(gameObject);
        }

        private void Update()
        {
            _mainThreadQueue.Execute(5);
        }

        private void UpdatePathfinding(NavMeshAgentComponent navMeshAgentComponent)
        {
            float currentTimeToCalculatePath = _timeToCalculatePath;
            
            DateTime start = DateTime.Now;

            ThreadResult<Vector3> threadOrigin = new ThreadResult<Vector3>();
            ThreadResult<Vector3> threadDestination = new ThreadResult<Vector3>();

            while (true)
            {
                DateTime end = DateTime.Now;
                
                currentTimeToCalculatePath += end.Millisecond - start.Millisecond;
                
                if (currentTimeToCalculatePath < _timeToCalculatePath)
                {
                    Debug.Log(currentTimeToCalculatePath);
                    continue;
                }

                currentTimeToCalculatePath = 0;

                if (_navMeshAgentDestinations[navMeshAgentComponent] == null)
                {
                    continue;
                }

                AStarPath aStarPath = _navMeshAgentDestinations[navMeshAgentComponent];
                
                _mainThreadQueue.GetPosition(navMeshAgentComponent.GetTransformComponent(), threadOrigin);
                _mainThreadQueue.GetPosition(aStarPath.position, threadDestination);

                _mainThreadQueue.SetAction(_updateAgentDestinationSystem.UpdateAgentDestination(navMeshAgentComponent, threadOrigin.GetValue(), 
                    threadDestination.GetValue(), aStarPath));
                
                start = DateTime.Now;
            }
        }

        public void AddNavMeshAgentEntity(NavMeshAgentComponent navMeshAgentComponent)
        {
            AStarPath aStarPath = new AStarPath(_navMeshGraph.Copy(), 
                new VectorComponent(navMeshAgentComponent.GetNavMeshAgent().destination));
            
            _navMeshAgentDestinations.Add(navMeshAgentComponent, aStarPath);

            Thread pathfindingThread = new Thread(() =>
            {
                UpdatePathfinding(navMeshAgentComponent);
            });
            _pathfindingThreads.Add(pathfindingThread);
            
            pathfindingThread.Start();
        }

        public void RemoveNavMeshAgentEntity(NavMeshAgentComponent navMeshAgentComponent)
        {
            _navMeshAgentDestinations.Remove(navMeshAgentComponent);
        }

        public IPosition GetNavMeshAgentDestination(NavMeshAgentComponent navMeshAgentComponent)
        {
            return _navMeshAgentDestinations[navMeshAgentComponent];
        }

        public void UpdateNavMeshAgentDestination(NavMeshAgentComponent navMeshAgentComponent, 
            VectorComponent vectorComponent)
        {
            _navMeshAgentDestinations[navMeshAgentComponent].position = vectorComponent;
        }

        public void UpdateNavMeshAgentDestination(NavMeshAgentComponent navMeshAgentComponent,
            TransformComponent transformComponent)
        {
            _navMeshAgentDestinations[navMeshAgentComponent].position = transformComponent;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;

            foreach (Node node in _navMeshGraph.nodes.Values)
            {
                Gizmos.DrawSphere(node.position, 0.2f);

                foreach (Edge edge in node.edges)
                {
                    Gizmos.DrawLine(edge.fromNode.position, edge.toNode.position);
                }
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _pathfindingThreads.Count; i++)
            {
                _pathfindingThreads[i].Abort();
            }
        }
    }
}
