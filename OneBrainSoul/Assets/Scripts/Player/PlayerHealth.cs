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

    public void Damage(int amount, GameObject source)
    {
        if (damageTime > 0f) return;
        health = Mathf.Max(0, health - amount);
        damageTime = damageCooldown;
        if (health == 0)
        {
            Die(source);
        }
        else
        {
            if (player.braincell)
            {
                PostProcessingManager.Instance.DamageEffect(damageEffectDuration);
                player.cam.ScreenShake(damageEffectDuration * .25f, 1f);
            }
        }
    }

    private void Update()
    {
        damageTime = Mathf.Max(0f, damageTime - Time.deltaTime);
    }

    public void Die(GameObject source)
    {

    }
}
