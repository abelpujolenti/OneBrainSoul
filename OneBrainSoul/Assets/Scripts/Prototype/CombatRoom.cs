using System.Collections;
using ECS.Entities.AI;
using ECS.Entities.AI.Combat;
using Player;
using UnityEngine;

public class CombatRoom : MonoBehaviour
{
    [SerializeField] AgentEntity[] enemies;
    [SerializeField] GameObject[] walls;
    [SerializeField] float delay = .5f;

    PlayerCharacterController player;
    bool active = false;
    bool beat = false;

    void Start()
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < walls.Length; i++)
        {
            DeactivateWall(walls[i]);
        }
    }

    void Update()
    {
        if (!active || beat) return;
        int e = 0;
        while (e < enemies.Length && enemies[e] == null)
        {
            e++;
        }
        if (e >= enemies.Length)
        {
            Beat();
        }
    }

    public void Enter(PlayerCharacterController player)
    {
        if (active || beat) return;
        active = true;
        this.player = player;
        StartCoroutine(EnterCoroutine(delay));
    }

    private IEnumerator EnterCoroutine(float t)
    {
        yield return new WaitForSeconds(t);
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].gameObject.SetActive(true);
        }
        for (int i = 0; i < walls.Length; i++)
        {
            ActivateWall(walls[i]);
        }
    }

    private void Beat()
    {
        beat = true;
        active = false;
        for (int i = 0; i < walls.Length; i++)
        {
            DeactivateWall(walls[i]);
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
