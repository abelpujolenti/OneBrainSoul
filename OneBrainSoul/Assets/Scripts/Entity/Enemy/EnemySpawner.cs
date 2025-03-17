using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS.Entities.AI;
using Managers;
using Player;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] bool spawnOnStart = true;
    [SerializeField] bool spawnOnEnterTrigger = false;
    public bool canSpawn = true;
    AgentEntity agentEntity;
    private void Awake()
    {
        agentEntity = transform.GetChild(0).GetComponent<AgentEntity>();
        agentEntity.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (spawnOnStart)
        {
            Spawn();
        }
    }

    public void Spawn()
    {
        if (!canSpawn) return;
        canSpawn = false;
        var spawnedAgent = Instantiate(agentEntity.transform, transform.transform.position, agentEntity.transform.rotation);
        spawnedAgent.localScale = agentEntity.transform.localScale;
        spawnedAgent.gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (spawnOnEnterTrigger && other.GetComponent<PlayerCharacterController>() != null)
        {
            Spawn();
        }
    }
}
