using Managers;
using Player.Movement;
using UnityEngine;

namespace Player.Abilities
{
    public class WallClimbAbility : MonoBehaviour
    {
        private bool _canClimb = true;
        private RaycastHit _hit;

        public void CheckWallClimb(PlayerCharacterController playerCharacterController)
        {
            if (playerCharacterController.GetMovementHandler() is WallClimbMovementHandler) return;

            if (playerCharacterController.IsOnTheGround() || 
                playerCharacterController.GetMovementHandler() is HookMovementHandler)
            {
                _canClimb = true;
            }

            if (!_canClimb || !playerCharacterController.HasPressedJump() || playerCharacterController.IsOnTheGround()) return;

            if (Physics.Raycast(playerCharacterController.transform.position + new Vector3(0f, 1.9f, 0f), 
                    playerCharacterController.GetOrientation().forward, 
                    out _hit, 
                    1f, 
                    GameManager.Instance.GetRaycastLayersWithoutAlly(), QueryTriggerInteraction.Ignore))
            {
                WallClimb(playerCharacterController);
            }
        }

        private void WallClimb(PlayerCharacterController playerCharacterController)
        {
            Vector3 forward = playerCharacterController.GetOrientation().forward;
                
            float viewDot = Vector3.Dot(_hit.normal, forward);
                
            float moveDot = Vector3.Dot(_hit.normal,
                playerCharacterController.GetOrientation().right * playerCharacterController.GetXInput() +
                forward * playerCharacterController.GetYInput());
                
            if (viewDot < -WallClimbMovementHandler.viewAngleThreshold && moveDot < -WallClimbMovementHandler.moveAngleThreshold)
            {
                _canClimb = false;
                playerCharacterController.ChangeMovementHandlerToWallClimb();
            }
        }

        public void EnableClimb()
        {
            _canClimb = true;
        }
    }
}
