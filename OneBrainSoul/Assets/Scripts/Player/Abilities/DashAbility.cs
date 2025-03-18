using Player.Movement;
using UnityEngine;
using System.Collections;

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
                _timesDashed < _airDashes &&
                playerCharacterController.GetCharges() > 0
                )
            {
                Dash(playerCharacterController);
            }
        }

        public void Dash(PlayerCharacterController player)
        {
            Vector3 forward = player.GetCamera().transform.forward;
            
            Quaternion r = player.GetOrientation().rotation;
            Vector3 dir = r * new Vector3(player.GetXInput(), 0f, player.GetYInput()).normalized;
            dir = dir == Vector3.zero ? new Vector3(forward.x, 0f, forward.z).normalized : dir;
            player.ChangeMovementHandlerToDash(dir);
            player.ResetAbility1Cooldown();

            _timesDashed++;
            player.ConsumeCharge();

            AudioManager.instance.PlayOneShot(FMODEvents.instance.dash, transform.position);
            StartCoroutine(AnimationCoroutine(player));
            player.GetAnimator().SetBool("Dash", true);
        }

        private IEnumerator AnimationCoroutine(PlayerCharacterController player)
        {
            yield return new WaitForSeconds(player.GetAnimator().GetCurrentAnimatorStateInfo(0).length);
            player.GetAnimator().SetBool("Dash", false);
        }

        public void ResetTimesDashed()
        {
            _timesDashed = 0;
        }
    }
}