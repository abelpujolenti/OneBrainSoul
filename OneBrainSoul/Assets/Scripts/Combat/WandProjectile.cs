using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WandProjectile : MonoBehaviour
{
    public Material material;
    PlayerCharacterController player;
    Wand wand;
    Vector3 direction;
    float speed;
    bool empowered = false;
    float lifeTime = 0f;
    float landTime = 0f;
    float deathTime = 0f;
    float deathDuration = .075f;
    float landDuration = .1f;
    float landExplosionScaleAdd = 8f;
    float originalScale;

    public void Init(PlayerCharacterController player, Wand wand, float lifeTime, Vector3 direction, float speed)
    {
        this.lifeTime = lifeTime;
        this.player = player;
        this.direction = direction;
        this.speed = speed;
        this.empowered = !player.onGround;
        this.wand = wand;
        originalScale = transform.localScale.x;
        material = GetComponent<MeshRenderer>().material;
        if (empowered)
        {
            material.SetColor("_Color", new Color(0f, .7f, 1f));
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (deathTime > 0f || landTime > 0f) return;
        EnemyTest enemy = collision.collider.GetComponent<EnemyTest>();
        if (enemy != null)
        {
            Land(enemy);
        }
        else
        {
            deathTime = deathDuration;
        }
    }

    private void FixedUpdate()
    {
        if (lifeTime > 0f && landTime == 0f && deathTime == 0f)
        {
            Vector3 movement = direction * speed * Time.fixedDeltaTime;
            transform.position += movement;
        }
    }

    private void Update()
    {
        if (lifeTime <= 0f && deathTime == 0f)
        {
            deathTime = deathDuration;
        }

        if (landTime > 0f)
        {
            float s = originalScale + Mathf.Pow(1f - landTime / landDuration, 2f) * landExplosionScaleAdd;
            transform.localScale = new Vector3(s, s, s);
            float o = Mathf.Pow(landTime / landDuration, 2f);
            material.SetFloat("_Opacity", o);
            if (landTime - Time.deltaTime <= 0f)
            {
                Destroy(gameObject);
            }
            
        }
        else if (deathTime > 0f)
        {
            float s = Mathf.Pow(deathTime / deathDuration, 2f);
            material.SetFloat("_Progress", s);
            if (deathTime - Time.deltaTime <= 0f)
            {
                Destroy(gameObject);
            }
        }

        landTime = Mathf.Max(0f, landTime - Time.deltaTime);
        deathTime = Mathf.Max(0f, deathTime - Time.deltaTime);
        lifeTime -= Time.deltaTime;
    }

    private void Land(EnemyTest enemy)
    {
        landTime = landDuration;
        enemy.Damage(player, transform.position, empowered ? 3 : 2);
        List<EnemyTest> enemies = new List<EnemyTest> { enemy };
        wand.ProjectileLanded(this, enemies);
    }
}
