using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DashAbility : MonoBehaviour
{
    PlayerCharacterController player;
    RaycastHit hit;
    private void Start()
    {
        player = GetComponent<PlayerCharacterController>();

    }
    private void Update()
    {
        if (!player.braincell) return;

        if (player.ability1Input && player.ability1Time == 0f)
        {
            Quaternion r = player.orientation.rotation;
            Vector3 dir = r * new Vector3(player.xInput, 0f, player.yInput).normalized;
            dir = dir == Vector3.zero ? new Vector3(player.cam.transform.forward.x, 0f, player.cam.transform.forward.z).normalized : dir;
            player.movementHandler = new DashMovementHandler(player, dir);
            player.ability1Time = player.ability1Cooldown;
        }
    }
}
