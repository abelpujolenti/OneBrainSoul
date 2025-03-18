using System.Collections.Generic;
using System.Collections;
using ECS.Entities.AI;
using Player;
using UnityEngine;
using System.Linq;

public class CombatRoom : MonoBehaviour
{
    List<EnemySpawner> spawners = new List<EnemySpawner>();
    List<AgentEntity> enemies = new List<AgentEntity>();
    [SerializeField] GameObject[] walls;
    [SerializeField] float delay = .5f;
    [SerializeField] float enemyDelay = .12f;

    PlayerCharacterController player;
    bool active = false;
    bool beat = false;
    bool spawned = false;

    void Start()
    {
        spawners = GetComponentsInChildren<EnemySpawner>().ToList();
        for (int i = 0; i < spawners.Count; i++)
        {
            enemies.Add(spawners[i].agentEntity);
        }
        for (int i = 0; i < walls.Length; i++)
        {
            DeactivateWall(walls[i]);
        }
    }

    void Update()
    {
        if (!active || beat) return;
        if (!spawned) return;
        beat = true;
        for (int i = 0; i < spawners.Count; i++)
        {
            if (spawners[i].HasLiveSpawns())
            {
                beat = false;
            }
        }
        if (beat)
        {
            Beat();
        }
    }

    public void Enter(PlayerCharacterController player)
    {
        if (active || beat) return;
        active = true;
        this.player = player;
        StartCoroutine(EnterCoroutine(delay, enemyDelay));
        player.GetComponent<PlayerCharacter>().EnterCombatRoom(this);
    }

    private IEnumerator EnterCoroutine(float t, float enemyDelay)
    {
        for (int i = 0; i < walls.Length; i++)
        {
            ActivateWall(walls[i]);
        }
        yield return new WaitForSeconds(t);
        for (int i = 0; i < enemies.Count; i++)
        {
            yield return new WaitForSeconds(enemyDelay);
            spawners[i].Spawn();
        }
        spawned = true;
    }

    private void Beat()
    {
        beat = true;
        active = false;
        for (int i = 0; i < walls.Length; i++)
        {
            DeactivateWall(walls[i]);
        }
        player.GetComponent<PlayerCharacter>().DefeatCombatRoom();
    }

    public void ResetRoom()
    {
        for (int i = 0; i < walls.Length; i++)
        {
            DeactivateWall(walls[i]);
        }

        beat = false;
        active = false;
        spawned = false;

        for (int i = 0; i < spawners.Count; i++)
        {
            spawners[i].ClearEntities();
            spawners[i].canSpawn = true;
        }
    }

    private void ActivateWall(GameObject w)
    {
        w.GetComponent<Collider>().enabled = true;
        var r = w.GetComponent<MeshRenderer>();
        Color c = r.material.GetColor("_Color");
        c.a = 1f;
        r.material.SetColor("_Color", c);
        r.material.SetInt("_Collapsed", 1);
    }

    private void DeactivateWall(GameObject w)
    {
        w.GetComponent<Collider>().enabled = false;
        var r = w.GetComponent<MeshRenderer>();
        Color c = r.material.GetColor("_Color");
        c.a = .5f;
        r.material.SetColor("_Color", c);
        r.material.SetInt("_Collapsed", 0);
        r.material.SetInt("_Active", beat ? 0 : 1);
    }
}
