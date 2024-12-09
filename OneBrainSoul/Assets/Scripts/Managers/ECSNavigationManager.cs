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

        private Dictionary<uint, Stopwatch> _stopwatches = new Dictionary<uint, Stopwatch>();
        private List<Queue<ElapsedTimePerAgent>> _agentsPerThread = new List<Queue<ElapsedTimePerAgent>>();

        private UpdateAgentDestinationSystem _updateAgentDestinationSystem = new UpdateAgentDestinationSystem();

        private Dictionary<uint, DynamicObstacle> _dynamicObstaclesID = new Dictionary<uint, DynamicObstacle>();

        private NavMeshGraph _navMeshGraph = new NavMeshGraph();

        private bool _active = true;

        private List<Mutex> mutexes = new List<Mutex>();
        
        [SerializeField]private uint _maxThreads = 3;
        private int _agentsCounter = 0;

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
                UpdateOwnPosition(aiAgentPath);
                
                UpdateDestinationPosition(aiAgentPath);
                
                UpdateDynamicObstaclesPositions(aiAgentPath.aStarPath);
                
                List<Node> path = aiAgentPath.aStarPath.path;
                
                if (path.Count == 0)
                {
                    continue;
                }
                
                CleanPreviousWayPoints(aiAgentPath.aStarPath.origin, path);

                Vector3 firstPathPosition = path[0].position;

                aiAgentPath.navMeshAgentComponent.GetNavMeshAgent().SetDestination(firstPathPosition);

                if (Vector3.Distance(aiAgentPath.navMeshAgentComponent.GetTransformComponent().GetPosition(), firstPathPosition) > 4f)
                {
                    continue;
                }

                path[^1].gCost -= path[0].gCost;
                
                path.RemoveAt(0);
            }
            
            _mainThreadQueue.Execute(5);
        }

        private void UpdateOwnPosition(AIAgentPath aiAgentPath)
        {
            Vector3 position = aiAgentPath.navMeshAgentComponent.GetTransformComponent().GetPosition();

            aiAgentPath.aStarPath.origin = position;
        }

        private void UpdateDestinationPosition(AIAgentPath aiAgentPath)
        {
            Vector3 destinationPosition = aiAgentPath.aStarPath.destinationPosition.GetPosition();

            destinationPosition += aiAgentPath.aStarPath.deviationVector;

            aiAgentPath.aStarPath.destination = destinationPosition;
        }

        private void UpdateDynamicObstaclesPositions(AStarPath aStarPath)
        {
            aStarPath.dynamicObstaclesPositions.Clear();
            
            foreach (DynamicObstacle dynamicObstacle in aStarPath.dynamicObstacles)
            {
                aStarPath.dynamicObstaclesPositions.Add(dynamicObstacle.iPosition.GetPosition());
            }
        }

        private void CleanPreviousWayPoints(Vector3 origin, List<Node> nodes)
        {
            if (nodes.Count < 2)
            {
                return;
            }

            float shortestDistance = Mathf.Infinity;
            float distanceToNode;

            int closestNodeIndex = 0;

            for (int i = 0; i < nodes.Count; i++)
            {
                distanceToNode = Vector3.Distance(nodes[i].position, origin);

                if (shortestDistance < distanceToNode)
                {
                    continue;
                }

                shortestDistance = distanceToNode;
                closestNodeIndex = i;
            }
            
            nodes.RemoveRange(0, closestNodeIndex);
        }

        private void UpdatePathfinding(int threadNum)
        {
            _agentsCounter++;

            Mutex mutex = mutexes[threadNum];

            while (_active)
            {
                uint agentID = 0;
                
                do
                {
                    mutex.WaitOne();
                    
                    foreach (ElapsedTimePerAgent elapsedTimePerAgent in _agentsPerThread[threadNum])
                    {
                        Stopwatch stopwatch = _stopwatches[elapsedTimePerAgent.agentID];
                    
                        if (stopwatch.ElapsedMilliseconds < _timeToCalculatePath)
                        {
                            continue;
                        }

                        agentID = elapsedTimePerAgent.agentID;
                        
                        _agentsPerThread[threadNum].Enqueue(_agentsPerThread[threadNum].Dequeue());
                    
                        stopwatch.Reset();
                        stopwatch.Start();
                        
                        break;
                    }
                    
                    mutex.ReleaseMutex();
                    
                } while (agentID == 0 && _active);
                
                AIAgentPath aiAgentPath = _navMeshAgentDestinations[agentID];
                    
                AStarPath aStarPath = aiAgentPath.aStarPath;

                if (aStarPath == null)
                {
                    continue;
                }
                    
                aStarPath.UpdateNavMeshGraphObstacles();

                _updateAgentDestinationSystem.UpdateAgentDestination(aStarPath);
                    
                aStarPath.GetNavMeshGraph().ResetEdgesCost();
            }
        }

        public void AddNavMeshAgentEntity(uint agentID, NavMeshAgentComponent navMeshAgentComponent, float radius)
        {
            DynamicObstacle dynamicObstacle = new DynamicObstacle
            {
                iPosition = navMeshAgentComponent.GetTransformComponent(),
                radius = radius + 4
            };
            
            AStarPath aStarPath = new AStarPath(new VectorComponent(navMeshAgentComponent.GetNavMeshAgent().destination),
                _navMeshGraph);
            
            AIAgentPath aiAgentPath = new AIAgentPath
            {
                navMeshAgentComponent = navMeshAgentComponent,
                aStarPath = aStarPath
            };

            AddDynamicObstacle(dynamicObstacle);
            
            LoadDynamicObstacles(aiAgentPath);
            
            _dynamicObstaclesID.Add(agentID, dynamicObstacle);
            
            _navMeshAgentDestinations.Add(agentID, aiAgentPath);

            ElapsedTimePerAgent elapsedTimePerAgent = new ElapsedTimePerAgent
            {
                agentID = agentID
            };

            Stopwatch stopwatch = new Stopwatch();
            
            stopwatch.Start();
            
            _stopwatches.Add(agentID, stopwatch);
            
            if (_agentsCounter > _maxThreads)
            {
                int index = (int)(_agentsCounter % (_maxThreads + 1));

                Mutex mutex = mutexes[index];

                mutex.WaitOne();
                
                _agentsPerThread[(int)(_agentsCounter % (_maxThreads + 1))].Enqueue(elapsedTimePerAgent);
                _agentsCounter++;
                
                mutex.ReleaseMutex();
                return;
            }

            _agentsPerThread.Add(new Queue<ElapsedTimePerAgent>(new[] {elapsedTimePerAgent}));
            
            mutexes.Add(new Mutex());

            Thread pathfindingThread = new Thread(() =>
            {
                UpdatePathfinding(_agentsCounter);
            });
            
            pathfindingThread.Start();
        }

        private void LoadDynamicObstacles(AIAgentPath aiAgentPath)
        {
            aiAgentPath.aStarPath.dynamicObstacles.AddRange(_dynamicObstaclesID.Values);
        }

        private void AddDynamicObstacle(DynamicObstacle dynamicObstacle)
        {
            foreach (AIAgentPath aiAgentPath in _navMeshAgentDestinations.Values)
            {
                aiAgentPath.aStarPath.dynamicObstacles.Add(dynamicObstacle);
            }
        }

        private void RemoveDynamicObstacle(DynamicObstacle dynamicObstacle)
        {
            foreach (AIAgentPath aiAgentPath in _navMeshAgentDestinations.Values)
            {
                aiAgentPath.aStarPath.dynamicObstacles.Remove(dynamicObstacle);
            }
        }

        public void RemoveNavMeshAgentEntity(uint agentID)
        {
            _navMeshAgentDestinations.Remove(agentID);
        }

        public IPosition GetNavMeshAgentDestination(uint agentID)
        {
            return _navMeshAgentDestinations[agentID].aStarPath;
        }

        public void UpdateNavMeshAgentDestination(uint agentID, VectorComponent vectorComponent)
        {
            _navMeshAgentDestinations[agentID].aStarPath.destinationPosition = vectorComponent;
            _navMeshAgentDestinations[agentID].aStarPath.deviationVector = Vector3.zero;
        }

        public void UpdateNavMeshAgentDestination(uint agentID, TransformComponent transformComponent)
        {
            _navMeshAgentDestinations[agentID].aStarPath.destinationPosition = transformComponent;
        }

        public void UpdateAStarDeviationVector(uint agentID, Vector3 deviationVector)
        {
            _navMeshAgentDestinations[agentID].aStarPath.deviationVector = deviationVector;
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

            foreach (AIAgentPath aiAgentPath in _navMeshAgentDestinations.Values)
            {
                Gizmos.color = Color.blue;

                AStarPath aStarPath = aiAgentPath.aStarPath;

                for (int i = 0; i < aStarPath.path.Count - 1; i++)
                {
                    Gizmos.DrawLine(aStarPath.path[i].position, aStarPath.path[i + 1].position);
                }
            }
        }*/

        public List<Vector3> GetPath(uint agentID)
        {
            List<Node> nodes = _navMeshAgentDestinations[agentID].aStarPath.path;

            List<Vector3> points = new List<Vector3>();

            foreach (Node node in nodes)
            {
                points.Add(node.position);
            }

            return points;
        }

        private void OnDestroy()
        {
            _active = false;
        }
    }
}
