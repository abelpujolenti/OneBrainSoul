using UnityEngine;

namespace Player.Abilities
{
    public class DashAbility : MonoBehaviour
    {
        [SerializeField] int airDashes = 1;
        PlayerCharacterController player;
        RaycastHit hit;
        int timesDashed = 0;

        private void Start()
        {
            player = GetComponent<PlayerCharacterController>();

        }
        private void Update()
        {
            if (player.onGround)
            {
                timesDashed = 0;
            }

            if (player.ability1Input && player.ability1Time == 0f && player.movementHandler is not HookMovementHandler && timesDashed < airDashes)
            {
                Dash();
            }
        }

        private void Dash()
        {
            Quaternion r = player.orientation.rotation;
            Vector3 dir = r * new Vector3(player.xInput, 0f, player.yInput).normalized;
            dir = dir == Vector3.zero ? new Vector3(player.cam.transform.forward.x, 0f, player.cam.transform.forward.z).normalized : dir;
            player.movementHandler = new DashMovementHandler(player, dir);
            player.ability1Time = player.ability1Cooldown;

            timesDashed++;
            AudioManager.instance.PlayOneShot(FMODEvents.instance.dash, transform.position);
        }
    }
}