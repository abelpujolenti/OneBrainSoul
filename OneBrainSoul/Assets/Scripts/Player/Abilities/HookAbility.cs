using Unity.VisualScripting;
using UnityEngine;

public class HookAbility : MonoBehaviour
{
    [SerializeField] Material hookshotChainMaterial;
    [SerializeField] float range = 100f;

    PlayerCharacterController player;
    RaycastHit hit;
    private void Start()
    {
        player = GetComponent<PlayerCharacterController>();
        var line = player.AddComponent<LineRenderer>();
        line.startWidth = 0.4f;
        line.endWidth = 0.3f;
        line.material = hookshotChainMaterial;
    }

    private void Update()
    {
        if (!player.braincell) return;

        Vector3 startPos, endPos;
        startPos = player.transform.position + new Vector3(0f, .5f, 0f);
        bool landed = Physics.Raycast(startPos, player.cam.transform.forward, out hit, range, 127, QueryTriggerInteraction.Ignore);

        if (landed)
        {
            if (player.ability1Time == 0f)
            {
                player.SetCrosshairColor(new Color(.9f, .9f, .1f));
            }
            else
            {
                player.SetCrosshairColor(new Color(.1f, .9f, .9f));
            }
        }
        else
        {
            player.SetCrosshairColor(new Color(1f, 1f, 1f));
        }


        if (player.ability1Input && player.ability1Time == 0f && landed)
        {
            endPos = hit.point;
            player.movementHandler = new HookMovementHandler(player, startPos, endPos);
            player.ability1Time = player.ability1Cooldown;
        }
    }
}
