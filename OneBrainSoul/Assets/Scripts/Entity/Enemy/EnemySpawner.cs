using System.Collections.Generic;
using ECS.Entities.AI;
using Player;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public bool spawnOnStart = true;
    public bool spawnOnEnterTrigger = false;
    public bool canSpawn = true;
    public AgentEntity agentEntity { get; private set; }
    private List<AgentEntity> spawnedEntities = new List<AgentEntity>();
    
    private void Awake()
    {
        agentEntity = transform.GetComponentInChildren<AgentEntity>();
        if (agentEntity == null)
        {
            agentEntity = transform.GetChild(0).GetComponentInChildren<AgentEntity>();
        }
        agentEntity.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (spawnOnStart)
        {
            Spawn();
        }
    }

    private void Update()
    {
        for (int i = spawnedEntities.Count - 1; i >= 0; i--)
        {
            if (spawnedEntities[i] == null)
            {
                spawnedEntities.RemoveAt(i);
            }
        }
    }

    public void ClearEntities()
    {
        for (int i = spawnedEntities.Count - 1; i >= 0; i--)
        {
            if (spawnedEntities[i] != null)
            {
                Destroy(spawnedEntities[i].gameObject);
                spawnedEntities.RemoveAt(i);
            }
        }
    }

    public void Spawn()
    {
        if (!canSpawn) return;
        canSpawn = false;
        var spawnedAgent = Instantiate(agentEntity.transform, agentEntity.transform.position, agentEntity.transform.rotation);
        spawnedAgent.localScale = agentEntity.transform.lossyScale;
        spawnedAgent.gameObject.SetActive(true);
        spawnedEntities.Add(spawnedAgent.GetComponent<AgentEntity>());
        AudioManager.Instance.PlayOneShot(FMODEvents.instance.enemySpawn, transform.position);
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
