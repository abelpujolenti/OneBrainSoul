using System.Collections.Generic;
using ECS.Entities.AI;
using Player;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public bool spawnOnStart = true;
    public bool spawnOnEnterTrigger = false;
    public bool canSpawn = true;
    public Transform transformToSpawn { get; private set; }

    private List<GameObject> spawnedEntities = new List<GameObject>();
    
    private void Awake()
    {
        transformToSpawn = transform.GetChild(0);
        transformToSpawn.gameObject.SetActive(false);
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
        Transform spawnedAgent = Instantiate(transformToSpawn, transformToSpawn.position, transformToSpawn.rotation);
        spawnedAgent.localScale = transformToSpawn.lossyScale;
        spawnedAgent.gameObject.SetActive(true);
        spawnedEntities.Add(spawnedAgent.gameObject);
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
