using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyBase : DamageTakingEntity
{
    [SerializeField] Transform damageParticlePrefab;
    ParticleSystem damageParticle;
    public float range = 12f;
    public float speed = 1.5f;
    public float rotSpeed = 30f;

    Material mat;
    protected Rigidbody rb;
    protected PlayerCharacterController[] playerCharacters;
    protected override void Start()
    {
        mat = GetComponentInChildren<MeshRenderer>().material;
        rb = GetComponent<Rigidbody>();
        playerCharacters = FindObjectsOfType<PlayerCharacterController>();
    }

    public override void Damage(PlayerCharacterController player, Vector3 hitPos, int amount = 1)
    {
        base.Damage(player, hitPos, amount);
        mat.SetColor("_DamageColor", new Color(1f, 0f, 0f));
        if (damageParticle == null)
        {
            damageParticle = Instantiate(damageParticlePrefab, transform.position, Quaternion.identity, transform).GetComponent<ParticleSystem>();
        }
        damageParticle.transform.position = hitPos;
        damageParticle.Play();
    }

    public void Knockback(PlayerCharacterController player)
    {
        rb.AddForce(player.orientation.forward * 2000f, ForceMode.Acceleration);
        rb.AddForce(Vector3.up * 1000f, ForceMode.Acceleration);
    }

    protected override void Update()
    {
        rb.AddForce(Vector3.down * 30f, ForceMode.Acceleration);
        if (damageParticle != null && !damageParticle.isPlaying)
        {
            Destroy(damageParticle.gameObject);
        }
        if (damageCooldown > 0 && damageCooldown - Time.deltaTime <= 0f)
        {
            mat.SetColor("_DamageColor", new Color(1f, 1f, 1f));
        }
        base.Update();
    }

    protected virtual void FixedUpdate()
    {
        BehaviorUpdate();
    }

    protected virtual void BehaviorUpdate()
    {

    }


}
