using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    PlayerCharacterController player;
    public int maxHealth = 100;
    public int maxShield = 2;
    public int health { get; private set; }
    public int shield { get; private set; }
    public float damageCooldown = .3f;
    public float damageEffectDuration = .2f;
    float damageTime;

    private void Start()
    {
        playerCharacterController = GetComponent<PlayerCharacterController>();
        health = maxHealth;
    }

    public bool Damage(int amount, GameObject source)
    {
        if (damageTime > 0f) return false;

        if (shield > 0)
        {
            shield--;
        }
        else
        {
            health = Mathf.Max(0, health - amount);
        }

        damageTime = damageCooldown;
        if (health == 0)
        {
            Die(source);
        }
        else
        {
            float kbDirHeight = .75f;
            Vector3 hitDir = (playerCharacterController.transform.position - source.transform.position).normalized;
            Vector3 kbDir = new Vector3(hitDir.x, kbDirHeight, hitDir.z).normalized;
            playerCharacterController.rb.AddForce(kbDir * 800f, ForceMode.Acceleration);

            if (playerCharacterController.movementHandler is GroundedMovementHandler)
            {
                playerCharacterController.movementHandler = new AirborneMovementHandler();
            }

            PostProcessingManager.Instance.DamageEffect(damageEffectDuration);
            playerCharacterController.cam.ScreenShake(damageEffectDuration * .25f, 1f);
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