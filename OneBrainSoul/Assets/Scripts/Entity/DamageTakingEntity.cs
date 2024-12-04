using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DamageTakingEntity : MonoBehaviour
{
    public int health = 1;
    public float radius = 1f;
    protected float damageCooldown = 0f;
    protected MeshCollider meshCollider;

    protected virtual void Start()
    {
        meshCollider = GetComponent<MeshCollider>();
    }

    public virtual void Damage(PlayerCharacterController player, Vector3 hitPos, int amount = 1)
    {
        if (damageCooldown > 0) return;
        damageCooldown = .2f;
        health -= amount;
        if (health <= 0)
        {
            Die(player, hitPos);
            return;
        }
    }

    protected virtual void Update()
    {
        damageCooldown = Mathf.Max(0f, damageCooldown - Time.deltaTime);
    }

    protected virtual void Die(PlayerCharacterController player, Vector3 hitPos)
    {
        Destroy(gameObject);
    }
}
