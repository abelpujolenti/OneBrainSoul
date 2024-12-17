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
        
        private Dictionary<uint, NavMeshAgentComponent> _navMeshAgentDestinations = 
            new Dictionary<uint, NavMeshAgentComponent>();

        private MainThreadQueue _mainThreadQueue = new MainThreadQueue();

        private Dictionary<uint, Stopwatch> _stopwatches = new Dictionary<uint, Stopwatch>();
        private List<List<uint>> _agentsPerThread = new List<List<uint>>();

        private UpdateAgentDestinationSystem _updateAgentDestinationSystem = new UpdateAgentDestinationSystem();

        private Dictionary<uint, DynamicObstacle> _dynamicObstaclesID = new Dictionary<uint, DynamicObstacle>();

        private NavMeshGraph _navMeshGraph = new NavMeshGraph();

        private bool _active = true;

        private List<Mutex> _mutexes = new List<Mutex>();
        
        [SerializeField]private uint _maxThreads = 3;

        [SerializeField]private bool _drawGizmos;

        [SerializeField]private uint ID;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;

                _timeToCalculatePath *= 1000;
                
                _navMeshGraph.LoadGraph();

                for (int i = 0; i < _maxThreads; i++)
                {
                    CreatePathfindingThead(i);
                }
                
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
            foreach (NavMeshAgentComponent navMeshAgentComponent in _navMeshAgentDestinations.Values)
            {
                UpdateOwnPosition(navMeshAgentComponent);
                
                UpdateDestinationPosition(navMeshAgentComponent.GetAStarPath());
                
                UpdateDynamicObstaclesPositions(navMeshAgentComponent.GetAStarPath());
                
                List<Node> path = navMeshAgentComponent.GetAStarPath().path;
                
                if (path.Count == 0)
                {
                    continue;
                }
                
                CleanPreviousWayPoints(navMeshAgentComponent.GetAStarPath().origin, path);

                Vector3 firstPathPosition = path[0].position;

                navMeshAgentComponent.GetNavMeshAgent().SetDestination(firstPathPosition);

                if (Vector3.Distance(navMeshAgentComponent.GetTransformComponent().GetPosition(), firstPathPosition) > 4f)
                {
                    continue;
                }

                path[^1].gCost -= path[0].gCost;
                
                path.RemoveAt(0);
            }
            
            //_mainThreadQueue.Execute(5);
        }

        private void UpdateOwnPosition(NavMeshAgentComponent navMeshAgentComponent)
        {
            Vector3 position = navMeshAgentComponent.GetTransformComponent().GetPosition();

            navMeshAgentComponent.GetAStarPath().origin = position;
        }

        private void UpdateDestinationPosition(AStarPath aStarPath)
        {
            Vector3 destinationPosition = aStarPath.destinationPosition.GetPosition();

            destinationPosition += aStarPath.deviationVector;

            aStarPath.destination = destinationPosition;
        }

        private void UpdateDynamicObstaclesPositions(AStarPath aStarPath)
        {
            aStarPath.dynamicObstaclesPositions.Clear();
            
            aStarPath.LockMutex();
            
            foreach (DynamicObstacle dynamicObstacle in aStarPath.dynamicObstacles)
            {
                aStarPath.dynamicObstaclesPositions.Add(dynamicObstacle.iPosition.GetPosition());
            }
            
            aStarPath.ReleaseMutex();
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
            Mutex mutex = _mutexes[threadNum];

            while (_active)
            {
                uint selectedAgentID = 0;
                
                do
                {
                    mutex.WaitOne();
                    
                    foreach (uint agentID in _agentsPerThread[threadNum])
                    {
                        Stopwatch stopwatch = _stopwatches[agentID];
                    
                        if (stopwatch.ElapsedMilliseconds < _timeToCalculatePath)
                        {
                            continue;
                        }

                        selectedAgentID = agentID;

                        _agentsPerThread[threadNum].Remove(selectedAgentID);
                        
                        _agentsPerThread[threadNum].Add(selectedAgentID);
                    
                        stopwatch.Reset();
                        stopwatch.Start();
                        
                        break;
                    }
                    
                    mutex.ReleaseMutex();
                    
                } while (selectedAgentID == 0 && _active);

                if (!_active)
                {
                    return;
                }
                    
                AStarPath aStarPath =  _navMeshAgentDestinations[selectedAgentID].GetAStarPath();

                if (aStarPath == null)
                {
                    continue;
                }
                    
                aStarPath.UpdateNavMeshGraphObstacles();

                _updateAgentDestinationSystem.UpdateAgentDestination(aStarPath, _triangleSideLength);
                    
                aStarPath.navMeshGraph.ResetGraphImportantInfo();
            }
        }

        public void AddNavMeshAgentEntity(uint agentID, NavMeshAgentComponent navMeshAgentComponent, float radius)
        {
            AddDynamicObstacle(navMeshAgentComponent, radius, navMeshAgentComponent.GetAStarPath(), agentID);

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();

            _stopwatches.Add(agentID, stopwatch);
            
            ReturnNavMeshAgentEntity(agentID, navMeshAgentComponent);

            _navMeshAgentDestinations[agentID].GetAStarPath().navMeshGraph = _navMeshGraph.DeepCopy();
        }

        public void ReturnNavMeshAgentEntity(uint agentID, NavMeshAgentComponent navMeshAgentComponent)
        {
            _navMeshAgentDestinations.Add(agentID, navMeshAgentComponent);
            
            AddAgentToThread(agentID, navMeshAgentComponent);
        }

        private void AddDynamicObstacle(NavMeshAgentComponent navMeshAgentComponent, float radius, AStarPath aStarPath,
            uint agentID)
        {
            DynamicObstacle dynamicObstacle = new DynamicObstacle
            {
                iPosition = navMeshAgentComponent.GetTransformComponent(),
                radius = radius + 4
            };
                
            NotifyNewDynamicObstacle(dynamicObstacle);

            LoadDynamicObstacles(aStarPath);

            _dynamicObstaclesID.Add(agentID, dynamicObstacle);
        }

        private void NotifyNewDynamicObstacle(DynamicObstacle dynamicObstacle)
        {
            AStarPath aStarPath;
            
            foreach (NavMeshAgentComponent navMeshAgentComponent in _navMeshAgentDestinations.Values)
            {
                aStarPath = navMeshAgentComponent.GetAStarPath();
                
                aStarPath.LockMutex();
                
                aStarPath.dynamicObstacles.Add(dynamicObstacle);
                
                aStarPath.ReleaseMutex();
            }
        }

        private void LoadDynamicObstacles(AStarPath aStarPath)
        {
            aStarPath.dynamicObstacles.AddRange(_dynamicObstaclesID.Values);
        }

        private void CreatePathfindingThead(int threadNum)
        {
            _agentsPerThread.Add(new List<uint>());

            _mutexes.Add(new Mutex());

            Thread pathfindingThread = new Thread(() =>
            {
                UpdatePathfinding(threadNum);
            });

            pathfindingThread.Start();
        }

        private void AddAgentToThread(uint agentID, NavMeshAgentComponent navMeshAgentComponent)
        {
            int index = 0;
            int lowestAgentsPerThread = _agentsPerThread[0].Count;
            int agentsInThread;
            
            for (int i = 1; i < _agentsPerThread.Count; i++)
            {
                agentsInThread = _agentsPerThread[i].Count;
                
                if (lowestAgentsPerThread <= agentsInThread)
                {
                    continue;
                }

                lowestAgentsPerThread = agentsInThread;
                index = i;
            }

            _mutexes[index].WaitOne();

            _agentsPerThread[index].Add(agentID);

            _mutexes[index].ReleaseMutex();
        }

        public void RemoveNavMeshAgentEntity(uint agentID, bool removeObstacle)
        {
            if (!_navMeshAgentDestinations.ContainsKey(agentID))
            {
                return;
            }
            
            _navMeshAgentDestinations.Remove(agentID);

            int index = 0;

            for (int i = 0; i < _agentsPerThread.Count; i++)
            {
                if (!_agentsPerThread[i].Contains(agentID))
                {
                    continue;
                }

                index = i;
                break;
            }

            _mutexes[index].WaitOne();

            foreach (List<uint> agentsList in _agentsPerThread)
            {
                if (!agentsList.Contains(agentID))
                {
                    continue;
                }

                agentsList.Remove(agentID);
                
                break;
            }

            _mutexes[index].ReleaseMutex();
            
            if (!removeObstacle)
            {
                return;
            }
            
            RemoveDynamicObstacle(agentID);
        }

        private void RemoveDynamicObstacle(uint obstacleID)
        {
            DynamicObstacle dynamicObstacle = _dynamicObstaclesID[obstacleID];

            AStarPath aStarPath;
            
            foreach (NavMeshAgentComponent navMeshAgentComponent in _navMeshAgentDestinations.Values)
            {
                aStarPath = navMeshAgentComponent.GetAStarPath();
                
                aStarPath.LockMutex();
                
                aStarPath.dynamicObstacles.Remove(dynamicObstacle);
                
                aStarPath.ReleaseMutex();
            }
        }

        public IPosition GetNavMeshAgentDestination(uint agentID)
        {
            return _navMeshAgentDestinations[agentID].GetAStarPath();
        }

        public void UpdateNavMeshAgentDestination(uint agentID, VectorComponent vectorComponent)
        {
            _navMeshAgentDestinations[agentID].GetAStarPath().destinationPosition = vectorComponent;
            _navMeshAgentDestinations[agentID].GetAStarPath().deviationVector = Vector3.zero;
        }

        public void UpdateNavMeshAgentDestination(uint agentID, TransformComponent transformComponent)
        {
            _navMeshAgentDestinations[agentID].GetAStarPath().destinationPosition = transformComponent;
        }

        public void UpdateAStarDeviationVector(uint agentID, Vector3 deviationVector)
        {
            _navMeshAgentDestinations[agentID].GetAStarPath().deviationVector = deviationVector;
        }

        private void OnDrawGizmos()
        {
            if (!_drawGizmos)
            {
                return;
            }
            
            Gizmos.color = Color.blue;
            
            /*foreach (Node node in _navMeshGraph.nodes.Values)
            {
                Gizmos.DrawSphere(node.position, 0.2f);

                foreach (Edge edge in node.edges)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(_navMeshGraph.nodes[edge.fromNodeIndex].position, _navMeshGraph.nodes[edge.toNodeIndex].position);
                }
            }*/
            
            foreach (Node node in _navMeshAgentDestinations[ID].GetAStarPath().navMeshGraph.nodes.Values)
            {
                Gizmos.DrawSphere(node.position, 0.2f);

                foreach (Edge edge in node.edges)
                {
                    Gizmos.color = new Color(1, edge.cost / 1000, edge.cost / 1000);
                    Gizmos.DrawLine(_navMeshGraph.nodes[edge.fromNodeIndex].position, _navMeshGraph.nodes[edge.toNodeIndex].position);
                }
            }
            
            Gizmos.color = Color.blue;

            foreach (NavMeshAgentComponent navMeshAgentComponent in _navMeshAgentDestinations.Values)
            {

                AStarPath aStarPath = navMeshAgentComponent.GetAStarPath();

                for (int i = 0; i < aStarPath.path.Count - 1; i++)
                {
                    Gizmos.DrawLine(aStarPath.path[i].position, aStarPath.path[i + 1].position);
                }
            }
        }

        public List<Vector3> GetPath(uint agentID)
        {
            if (!_navMeshAgentDestinations.ContainsKey(agentID))
            {
                return new List<Vector3>();
            }
            
            List<Node> nodes = _navMeshAgentDestinations[agentID].GetAStarPath().path;

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
