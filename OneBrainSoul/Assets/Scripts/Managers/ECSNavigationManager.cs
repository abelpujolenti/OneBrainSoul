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
using Debug = UnityEngine.Debug;

namespace Managers
{
    public class ECSNavigationManager : MonoBehaviour
    {
        private static ECSNavigationManager _instance;

        public static ECSNavigationManager Instance => _instance;

        [SerializeField] private float _triangleSideLength;

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
                
                _navMeshGraph.LoadGraph();
                
                DontDestroyOnLoad(gameObject);
                
                return;
            }
            
            Destroy(gameObject);
        }

        public void BakeGraph()
        {
            _navMeshGraph.BuildGraph(NavMesh.CalculateTriangulation(), _triangleSideLength);
        }

        public void EraseGraph()
        {
            _navMeshGraph.EraseGraphFile();
            _navMeshGraph = new NavMeshGraph();
        }

        private void Update()
        {
            foreach (var navMeshAgentDestination in _navMeshAgentDestinations)
            {
                List<Node> path = navMeshAgentDestination.Value.path;
                
                if (path.Count == 0)
                {
                    continue;
                }

                Vector3 position = path[0].position;

                navMeshAgentDestination.Key.GetNavMeshAgent().SetDestination(position);

                if (Vector3.Distance(navMeshAgentDestination.Key.GetTransformComponent().GetPosition(), position) > 3f)
                {
                    continue;
                }

                path[^1].gCost -= path[0].gCost;
                
                path.RemoveAt(0);
            }
            
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

                _updateAgentDestinationSystem.UpdateAgentDestination(threadOrigin.GetValue(), threadDestination.GetValue(), 
                    aStarPath);
            }
        }

        public static int counter = 0;

        public void AddNavMeshAgentEntity(NavMeshAgentComponent navMeshAgentComponent)
        {
            AStarPath aStarPath = new AStarPath(new VectorComponent(navMeshAgentComponent.GetNavMeshAgent().destination));
            
            _navMeshAgentDestinations.Add(navMeshAgentComponent, aStarPath);
            
            if (counter > 0)
            {
                return;
            }

            counter++;

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

        /*private void OnDrawGizmos()
        {
            foreach (Node node in _navMeshGraph.nodes.Values)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(node.position, 0.2f);

                foreach (Edge edge in node.edges)
                {
                    Gizmos.color = new Color(1, edge.cost / 1000, edge.cost / 1000);
                    Gizmos.DrawLine(edge.fromNode.position, edge.toNode.position);
                }
            }

            foreach (AStarPath aStarPath in _navMeshAgentDestinations.Values)
            {
                Gizmos.color = Color.blue;

                for (int i = 0; i < aStarPath.path.Count - 1; i++)
                {
                    Gizmos.DrawLine(aStarPath.path[i].position, aStarPath.path[i + 1].position);
                }
            }
        }*/

        public void UpdateNavMeshAgentPosition(NavMeshAgentComponent navMeshAgentComponent, float radius)
        {
            foreach (NavMeshAgentComponent navMeshAgent in _navMeshAgentDestinations.Keys)
            {
                if (navMeshAgent == navMeshAgentComponent)
                {
                    continue;
                }
                _navMeshGraph.ResetEdgesCost();
                _navMeshGraph.UpdateEdgeWeights(
                    navMeshAgentComponent.GetTransformComponent().GetPosition(), radius, 100000000000);
                _navMeshAgentDestinations[navMeshAgent].navMeshGraph.ResetEdgesCost();
                _navMeshAgentDestinations[navMeshAgent].navMeshGraph.UpdateEdgeWeights(
                    navMeshAgentComponent.GetTransformComponent().GetPosition(), radius, 100000000000);
            }
        }

        private void OnDestroy()
        {
            _active = false;
        }
    }
}
