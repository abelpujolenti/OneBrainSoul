using ECS.Entities.AI;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    AgentEntity agentEntity;
    private void Awake()
    {
        agentEntity = transform.GetChild(0).GetComponent<AgentEntity>();
    }
}
