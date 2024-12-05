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

namespace Managers
{
    public class ECSNavigationManager : MonoBehaviour
    {
        private static ECSNavigationManager _instance;

        public static ECSNavigationManager Instance => _instance;

        [SerializeField] private float _triangleSideLength;

        [SerializeField] private float _timeToCalculatePath;
        
        private Dictionary<uint, AIAgentPath> _navMeshAgentDestinations = 
            new Dictionary<uint, AIAgentPath>();

        private MainThreadQueue _mainThreadQueue = new MainThreadQueue();

        private List<List<uint>> _agentsPerThread = new List<List<uint>>();

        private UpdateAgentDestinationSystem _updateAgentDestinationSystem = new UpdateAgentDestinationSystem();

        private List<DynamicObstacle> _dynamicObstacles = new List<DynamicObstacle>();

        private NavMeshGraph _navMeshGraph = new NavMeshGraph();

        private bool _active = true;

        [SerializeField]private uint _maxThreads = 3;
        private int _threadsCounter = 0;

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
            foreach (AIAgentPath aiAgentPath in _navMeshAgentDestinations.Values)
            {
                List<Node> path = aiAgentPath.aStarPath.path;
                
                if (path.Count == 0)
                {
                    continue;
                }

                Vector3 position = path[0].position;

                aiAgentPath.navMeshAgentComponent.GetNavMeshAgent().SetDestination(position);

                if (Vector3.Distance(aiAgentPath.navMeshAgentComponent.GetTransformComponent().GetPosition(), position) > 3f)
                {
                    continue;
                }

                path[^1].gCost -= path[0].gCost;
                
                path.RemoveAt(0);
            }
            
            _mainThreadQueue.Execute(5);
        }

        private void UpdatePathfinding(int threadNum)
        {
            _threadsCounter++;
            
            Stopwatch counter = new Stopwatch();
            
            counter.Start();

            ThreadResult<Vector3> threadOrigin = new ThreadResult<Vector3>();
            ThreadResult<Vector3> threadDestination = new ThreadResult<Vector3>();
            ThreadResult<List<Vector3>> threadDynamicObstacles = new ThreadResult<List<Vector3>>();

            while (_active)
            {
                if (counter.ElapsedMilliseconds < _timeToCalculatePath)
                {
                    continue;
                }
                
                counter.Reset();
                counter.Start();

                foreach (uint combatAgentID in _agentsPerThread[threadNum])
                {
                    AIAgentPath aiAgentPath = _navMeshAgentDestinations[combatAgentID];
                    
                    AStarPath aStarPath = aiAgentPath.aStarPath;

                    if (aStarPath == null)
                    {
                        continue;
                    }

                    List<IPosition> iPositions = new List<IPosition>();

                    foreach (DynamicObstacle dynamicObstacle in _dynamicObstacles)
                    {
                        iPositions.Add(dynamicObstacle.iPosition);
                    }
                
                    _mainThreadQueue.GetPosition(aiAgentPath.navMeshAgentComponent.GetTransformComponent(), threadOrigin);
                    _mainThreadQueue.GetPosition(aStarPath.destinationPosition, threadDestination);
                    _mainThreadQueue.GetPositions(iPositions, threadDynamicObstacles);

                    for (int i = 0; i < _dynamicObstacles.Count; i++)
                    {
                        if (_dynamicObstacles[i].agentId == combatAgentID)
                        {
                            continue;
                        }
                        aStarPath.navMeshGraph.UpdateEdgeWeights(threadDynamicObstacles.GetValue()[i], 
                            _dynamicObstacles[i].radius, 100000000000);
                    }

                    _updateAgentDestinationSystem.UpdateAgentDestination(threadOrigin.GetValue(), threadDestination.GetValue(), 
                        aStarPath);
                    
                    aStarPath.navMeshGraph.ResetEdgesCost();
                }
            }
        }

        public void AddNavMeshAgentEntity(uint agentID, NavMeshAgentComponent navMeshAgentComponent, float radius)
        {
            DynamicObstacle dynamicObstacle = new DynamicObstacle
            {
                agentId = agentID,
                iPosition = navMeshAgentComponent.GetTransformComponent(),
                radius = radius + 10
            };
            
            _dynamicObstacles.Add(dynamicObstacle);
            
            AStarPath aStarPath = new AStarPath(new VectorComponent(navMeshAgentComponent.GetNavMeshAgent().destination));
            
            _navMeshAgentDestinations.Add(agentID, new AIAgentPath
            {
                navMeshAgentComponent = navMeshAgentComponent,
                aStarPath = aStarPath
            });
            
            if (_threadsCounter > _maxThreads)
            {
                return;
            }
            
            _agentsPerThread.Add(new List<uint>
            {
                agentID
            });

            Thread pathfindingThread = new Thread(() =>
            {
                UpdatePathfinding(_threadsCounter);
            });
            
            pathfindingThread.Start();
        }

        public void RemoveNavMeshAgentEntity(uint combatAgentID)
        {
            _navMeshAgentDestinations.Remove(combatAgentID);
        }

        public IPosition GetNavMeshAgentDestination(uint combatAgentID)
        {
            return _navMeshAgentDestinations[combatAgentID].aStarPath;
        }

        public void UpdateNavMeshAgentDestination(uint combatAgentID, VectorComponent vectorComponent)
        {
            _navMeshAgentDestinations[combatAgentID].aStarPath.destinationPosition = vectorComponent;
        }

        public void UpdateNavMeshAgentDestination(uint combatAgentID, TransformComponent transformComponent)
        {
            _navMeshAgentDestinations[combatAgentID].aStarPath.destinationPosition = transformComponent;
        }

        private void OnDrawGizmos()
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

            foreach (AIAgentPath aiAgentPath in _navMeshAgentDestinations.Values)
            {
                Gizmos.color = Color.blue;

                AStarPath aStarPath = aiAgentPath.aStarPath;

                for (int i = 0; i < aStarPath.path.Count - 1; i++)
                {
                    Gizmos.DrawLine(aStarPath.path[i].position, aStarPath.path[i + 1].position);
                }
            }
        }

        private void OnDestroy()
        {
            _active = false;
        }
    }
}
