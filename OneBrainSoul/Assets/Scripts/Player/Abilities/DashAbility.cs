using Player.Movement;
using UnityEngine;

namespace Player.Abilities
{
    public class DashAbility : MonoBehaviour
    {
        [SerializeField] private int _airDashes = 1;
        
        private RaycastHit _hit;
        private int _timesDashed = 0;

        public void CheckDash(PlayerCharacterController playerCharacterController, bool ability1Input)
        {
            if (ability1Input && 
                playerCharacterController.GetAbility1Time() == 0f && 
                playerCharacterController.GetMovementHandler() is not HookMovementHandler && 
                _timesDashed < _airDashes)
            {
                Dash(playerCharacterController);
            }
        }

        public void Dash(PlayerCharacterController playerCharacterController)
        {
            Vector3 forward = playerCharacterController.GetCamera().transform.forward;
            
            Quaternion r = playerCharacterController.GetOrientation().rotation;
            Vector3 dir = r * new Vector3(playerCharacterController.GetXInput(), 0f, playerCharacterController.GetYInput()).normalized;
            dir = dir == Vector3.zero ? new Vector3(forward.x, 0f, forward.z).normalized : dir;
            playerCharacterController.ChangeMovementHandlerToDash(dir);
            playerCharacterController.ResetAbility1Cooldown();

            _timesDashed++;
            AudioManager.instance.PlayOneShot(FMODEvents.instance.dash, transform.position);
        }

        public void ResetTimesDashed()
        {
            _timesDashed = 0;
        }
    }
}