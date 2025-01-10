using ECS.Entities.AI.Navigation;
using Managers;
using UnityEngine;

namespace DEBUG
{
    public class DEBUG_DynamicObstacle : NavMeshAgentEntity
    {
        // Start is called before the first frame update
        void Start()
        {
            Setup();
            
            ECSNavigationManager.Instance.AddObstacle(transform, GetComponent<CapsuleCollider>().radius);
        }
    }
}
