using Managers;
using UnityEngine;

namespace Player.Abilities
{
    public class WallClimbAbility : MonoBehaviour
    {
        bool canClimb = true;
        PlayerCharacterController player;
        RaycastHit hit;

        private void Start()
        {
            player = GetComponent<PlayerCharacterController>();

        }
        private void Update()
        {
            if (player.movementHandler is WallClimbMovementHandler) return;

            if (player.onGround || player.movementHandler is HookMovementHandler)
            {
                canClimb = true;
            }

            if (!canClimb || !player.jumpInput || player.onGround) return;

            if (Physics.Raycast(player.transform.position + new Vector3(0f, 1.9f, 0f), player.orientation.forward, out hit, 1f, GameManager.Instance.GetRaycastLayersWithoutAlly(), QueryTriggerInteraction.Ignore))
            {
                float viewDot = Vector3.Dot(hit.normal, player.orientation.forward);
                float moveDot = Vector3.Dot(hit.normal, player.orientation.right * player.xInput + player.orientation.forward * player.yInput);
                if (viewDot < -WallClimbMovementHandler.viewAngleThreshold && moveDot < -WallClimbMovementHandler.moveAngleThreshold)
                {
                    canClimb = false;
                    player.movementHandler = new WallClimbMovementHandler();
                }
            }
        }
    }
}
