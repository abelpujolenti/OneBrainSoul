using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    PlayerCharacterController player;
    public int maxHealth = 20;
    public int health { get; private set; }
    public float damageCooldown = .3f;
    public float damageEffectDuration = .2f;
    float damageTime;

    private void Start()
    {
        player = GetComponent<PlayerCharacterController>();
        health = maxHealth;
    }

    public bool Damage(int amount, GameObject source)
    {
        if (damageTime > 0f) return false;
        health = Mathf.Max(0, health - amount);
        damageTime = damageCooldown;
        if (health == 0)
        {
            Die(source);
        }
        else
        {
            float kbDirHeight = .75f;
            Vector3 hitDir = (player.transform.position - source.transform.position).normalized;
            Vector3 kbDir = new Vector3(hitDir.x, kbDirHeight, hitDir.z).normalized;
            player.rb.AddForce(kbDir * 800f, ForceMode.Acceleration);

            if (player.movementHandler is GroundedMovementHandler)
            {
                player.movementHandler = new AirborneMovementHandler();
            }

            if (player.braincell)
            {
                PostProcessingManager.Instance.DamageEffect(damageEffectDuration);
                player.cam.ScreenShake(damageEffectDuration * .25f, 1f);
            }
        }
        return true;
    }

    private void Update()
    {
        damageTime = Mathf.Max(0f, damageTime - Time.deltaTime);
    }

    public void Die(GameObject source)
    {

    }
}