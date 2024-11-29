using System.Collections.Generic;
using ECS.Components.AI.Navigation;
using ECS.Systems.AI.Navigation;
using Interfaces.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace Managers
{
    public class ECSNavigationManager : MonoBehaviour
    {
        private static ECSNavigationManager _instance;

        public static ECSNavigationManager Instance => _instance;
        
        private Dictionary<NavMeshAgentComponent, IPosition> _navMeshAgentDestinations = 
            new Dictionary<NavMeshAgentComponent, IPosition>();

        private UpdateAgentDestinationSystem _updateAgentDestinationSystem = new UpdateAgentDestinationSystem();

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                
                _updateAgentDestinationSystem.navMeshGraph.BuildGraph(NavMesh.CalculateTriangulation());;
                
                DontDestroyOnLoad(gameObject);
                
                return;
            }
            
            Destroy(gameObject);
        }

        private void Update()
        {
            foreach (var navMeshAgentDestination in _navMeshAgentDestinations)
            {
                if (navMeshAgentDestination.Value == null)
                {
                    continue;
                }
                _updateAgentDestinationSystem.UpdateAgentDestination(navMeshAgentDestination.Key, navMeshAgentDestination.Value);
            }
        }

        public void AddNavMeshAgentEntity(NavMeshAgentComponent navMeshAgentComponent)
        {
            _navMeshAgentDestinations.Add(navMeshAgentComponent,
                new VectorComponent(navMeshAgentComponent.GetNavMeshAgent().destination));
        }

        public void RemoveNavMeshAgentEntity(NavMeshAgentComponent navMeshAgentComponent)
        {
            _navMeshAgentDestinations.Remove(navMeshAgentComponent);
        }

        public IPosition GetNavMeshAgentDestination(NavMeshAgentComponent navMeshAgentComponent)
        {
            return _navMeshAgentDestinations[navMeshAgentComponent];
        }

        public void UpdateNavMeshAgentVectorDestination(NavMeshAgentComponent navMeshAgentComponent, 
            VectorComponent vectorComponent)
        {
            _navMeshAgentDestinations[navMeshAgentComponent] = vectorComponent;
        }

        public void UpdateNavMeshAgentTransformDestination(NavMeshAgentComponent navMeshAgentComponent,
            TransformComponent transformComponent)
        {
            _navMeshAgentDestinations[navMeshAgentComponent] = transformComponent;
        }
    }
}
