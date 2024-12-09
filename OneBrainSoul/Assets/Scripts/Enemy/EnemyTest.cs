using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyTest : MonoBehaviour
{
    [SerializeField] Transform damageParticlePrefab;
    ParticleSystem damageParticle;
    public int hp = 5;
    public float radius = .5f;
    public float range = 12f;
    public float speed = 1.5f;
    public float rotSpeed = 30f;

    Material mat;
    float damageCooldown = 0f;
    Rigidbody rb;
    PlayerCharacterController[] playerCharacters;
    private void Start()
    {
        mat = GetComponentInChildren<MeshRenderer>().material;
        rb = GetComponent<Rigidbody>();
        playerCharacters = FindObjectsOfType<PlayerCharacterController>();
    }

    public void Damage(PlayerCharacterController player, Vector3 hitPos, int amount = 1)
    {
        if (damageCooldown > 0) return;
        hp -= amount;
        mat.SetColor("_DamageColor", new Color(1f, 0f, 0f));
        damageCooldown = .2f;
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

    private void Update()
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
        if (damageCooldown < 0.2f && hp <= 0)
        {
            Destroy(gameObject);
            return;
        }
        damageCooldown = Mathf.Max(0f, damageCooldown - Time.deltaTime);
    }

    private void FixedUpdate()
    {
        PlayerCharacterController followedPlayer = null;
        foreach (PlayerCharacterController player in playerCharacters)
        {

            float minD = float.PositiveInfinity;
            float d = Vector3.Distance(player.transform.position, transform.position);
            followedPlayer = d < minD && d < range ? player : followedPlayer;
            minD = d < minD ? d : minD;
        }
        if (followedPlayer != null)
        {
            FollowUpdate(followedPlayer);
        }
    }

    void FollowUpdate(PlayerCharacterController player)
    {
        Vector3 plVector = (player.transform.position - transform.position);
        plVector.y = 0f;
        if (plVector.magnitude > radius + 2f && damageCooldown <= 0)
        {
            transform.rotation = Quaternion.LookRotation(plVector.normalized);
            transform.position += plVector.normalized * Time.deltaTime * speed;
        }
    }
}
