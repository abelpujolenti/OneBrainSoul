using UnityEngine;

public class ChargeAbility : MonoBehaviour
{
    PlayerCharacterController player;
    private void Start()
    {
        player = GetComponent<PlayerCharacterController>();
    }

    private void Update()
    {
        if (!player.braincell) return;

        if (player.onGround && player.ability1Input && player.ability1Time == 0f)
        {
            player.movementHandler = new ChargeMovementHandler(player, player.orientation.forward);
            player.ability1Time = player.ability1Cooldown;
            AudioManager.instance.PlayOneShot(FMODEvents.instance.charge, transform.position);
        }
    }
}
