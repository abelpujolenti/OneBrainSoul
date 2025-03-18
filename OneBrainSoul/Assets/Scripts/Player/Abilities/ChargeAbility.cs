using UnityEngine;

namespace Player.Abilities
{
    public class ChargeAbility : MonoBehaviour
    {
        public void CheckCharge(PlayerCharacterController playerCharacterController, bool ability1Input)
        {
            if (playerCharacterController.IsOnTheGround() && ability1Input && playerCharacterController.GetAbility1Time() == 0)
            {
                playerCharacterController.ChangeMovementHandlerToCharge();
                playerCharacterController.ResetAbility1Cooldown();
                AudioManager.instance.PlayOneShot(FMODEvents.instance.charge, transform.position);
            }
        }
    }
}
