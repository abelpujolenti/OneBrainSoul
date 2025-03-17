using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS.Entities.AI;

public class EnemySpawner : MonoBehaviour
{
    AgentEntity agentEntity;
    private void Awake()
    {
        agentEntity = transform.GetChild(0).GetComponent<AgentEntity>();
    }
}
