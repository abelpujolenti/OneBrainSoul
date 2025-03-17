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
    public AgentEntity agentEntity { get; private set; }
    private List<AgentEntity> spawnedEntities = new List<AgentEntity>();
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

    private void LateUpdate()
    {
        for (int i = spawnedEntities.Count - 1; i >= 0; i--)
        {
            if (spawnedEntities[i] = null)
            {
                spawnedEntities.RemoveAt(i);
            }
        }
    }

    public void Spawn()
    {
        if (!canSpawn) return;
        canSpawn = false;
        var spawnedAgent = Instantiate(agentEntity.transform, agentEntity.transform.position, agentEntity.transform.rotation);
        spawnedAgent.localScale = agentEntity.transform.localScale;
        spawnedAgent.gameObject.SetActive(true);
        spawnedEntities.Add(spawnedAgent.GetComponent<AgentEntity>());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (spawnOnEnterTrigger && other.GetComponent<PlayerCharacterController>() != null)
        {
            Spawn();
        }
    }

    public bool HasLiveSpawns()
    {
        return spawnedEntities.Count > 0;
    }
}
