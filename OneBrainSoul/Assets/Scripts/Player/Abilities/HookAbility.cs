using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class HookAbility : MonoBehaviour
{
    [SerializeField] Material hookshotChainMaterial;
    [SerializeField] float range = 100f;
    [SerializeField] float radius = .5f;
    [SerializeField] float ledgeSnapLeniency = 4f;
    [SerializeField] float ledgeSnapNormalThreshold = .5f;
    [SerializeField] float ledgeSnapDistance = 1f;

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
        bool landed = Physics.SphereCast(startPos, radius, player.cam.transform.forward, out hit, range, 127, QueryTriggerInteraction.Ignore);

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
            RaycastHit hit2;
            ledgeSnapLeniency = 4f;
            bool ledgeFound = Physics.SphereCast(hit.point + ledgeSnapLeniency * Vector3.up, .1f, Vector3.down, out hit2, ledgeSnapLeniency, 127, QueryTriggerInteraction.Ignore);
            ledgeFound &= hit2.point != Vector3.zero;
            ledgeFound &= Vector3.Dot(Vector3.up, hit2.normal) > ledgeSnapNormalThreshold;
            endPos = hit.point;
            if (ledgeFound) {
                Vector3 dir = (hit2.point - startPos).normalized;
                endPos = hit2.point + new Vector3(0f, .6f, 0f) + (dir + hit2.normal).normalized * ledgeSnapDistance;
            }

            player.movementHandler = new HookMovementHandler(player, startPos, endPos, ledgeFound ? hit2.point: hit.point, ledgeFound);
            player.ability1Time = player.ability1Cooldown;
        }
    }
}
