using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveBarrel : DamageTakingEntity
{
    [SerializeField] float explosionPower = 10000f;
    [SerializeField] float explosionRadius = 5f;
    [SerializeField] float explosionFalloff = 2f;

    protected override void Die(PlayerCharacterController player, Vector3 hitPos)
    {
        Explode();
        base.Die(player, hitPos);
    }

    void Explode()
    {
        var colliders = Physics.OverlapCapsule(transform.position, transform.position + meshCollider.bounds.size.y * Vector3.up, radius + explosionRadius);
        foreach (var collider in colliders)
        {
            if (collider.attachedRigidbody != null && (collider.GetComponent<PlayerCharacterController>() != null || collider.GetComponent<DamageTakingEntity>() != null))
            {
                ApplyExplosionForce(collider.attachedRigidbody);
            }
        }
    }

    void ApplyExplosionForce(Rigidbody receiver)
    {
        Vector3 d = (receiver.transform.position - transform.position);
        float power = Mathf.Pow(Mathf.Clamp01(1f - (d.magnitude / (explosionRadius + radius))), 1f / explosionFalloff);
        receiver.AddForce(d.normalized * explosionPower * power, ForceMode.Acceleration);
    }
}
