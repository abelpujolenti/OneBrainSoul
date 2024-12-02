using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using AI.Combat.CombatNavigation;
using AI.Navigation;
using ECS.Components.AI.Navigation;
using ECS.Systems.AI.Navigation;
using Interfaces.AI.Navigation;
using Threads;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Managers
{
    public class ECSNavigationManager : MonoBehaviour
    {
        private static ECSNavigationManager _instance;

        public static ECSNavigationManager Instance => _instance;

        [SerializeField] private float _triangleArea;

        [SerializeField] private float _timeToCalculatePath;
        
        private Dictionary<NavMeshAgentComponent, AStarPath> _navMeshAgentDestinations = 
            new Dictionary<NavMeshAgentComponent, AStarPath>();

        private MainThreadQueue _mainThreadQueue = new MainThreadQueue();

        private UpdateAgentDestinationSystem _updateAgentDestinationSystem = new UpdateAgentDestinationSystem();

        private NavMeshGraph _navMeshGraph = new NavMeshGraph();

        private bool _active = true;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;

                _timeToCalculatePath *= 1000;

                _navMeshGraph.BuildGraph(NavMesh.CalculateTriangulation(), _triangleArea);
                
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
            Stopwatch counter = new Stopwatch();
            
            counter.Start();

            ThreadResult<Vector3> threadOrigin = new ThreadResult<Vector3>();
            ThreadResult<Vector3> threadDestination = new ThreadResult<Vector3>();

            while (_active)
            {
                if (counter.ElapsedMilliseconds < _timeToCalculatePath)
                {
                    continue;
                }
                
                counter.Reset();
                counter.Start();

                if (_navMeshAgentDestinations[navMeshAgentComponent] == null)
                {
                    continue;
                }

                AStarPath aStarPath = _navMeshAgentDestinations[navMeshAgentComponent];
                
                _mainThreadQueue.GetPosition(navMeshAgentComponent.GetTransformComponent(), threadOrigin);
                _mainThreadQueue.GetPosition(aStarPath.position, threadDestination);

                _mainThreadQueue.SetAction(_updateAgentDestinationSystem.UpdateAgentDestination(navMeshAgentComponent, threadOrigin.GetValue(), 
                    threadDestination.GetValue(), aStarPath));
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

        private void OnDestroy()
        {
            _active = false;
        }
    }
}
