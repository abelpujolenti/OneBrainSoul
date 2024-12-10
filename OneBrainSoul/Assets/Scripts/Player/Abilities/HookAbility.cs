using Managers;
using Unity.VisualScripting;
using UnityEngine;

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
        if (player.movementHandler is HookMovementHandler)
        {
            (player.movementHandler as HookMovementHandler).VisualUpdate(player);
        }

        if (!player.braincell) return;

        Vector3 startPos, endPos;
        Vector3 dir = player.cam.transform.forward;
        startPos = player.transform.position + new Vector3(0f, .5f, 0f);
        bool landed = Physics.SphereCast(startPos + dir * 2f, radius, dir, out hit, range, GameManager.Instance.GetRaycastLayersWithoutAlly(), QueryTriggerInteraction.Ignore);

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

            RaycastHit ledgeHit;
            ledgeSnapLeniency = 4f;
            Vector3 dirNoY = new Vector3(dir.x, 0f, dir.z).normalized; 
            bool ledgeFound = Physics.SphereCast(hit.point + dirNoY * .15f + ledgeSnapLeniency * Vector3.up, .5f, Vector3.down, out ledgeHit, ledgeSnapLeniency, GameManager.Instance.GetRaycastLayersWithoutAlly(), QueryTriggerInteraction.Ignore);
            ledgeFound &= ledgeHit.point != Vector3.zero;
            ledgeFound &= Vector3.Dot(Vector3.up, ledgeHit.normal) > ledgeSnapNormalThreshold;
            ledgeFound &= Mathf.Abs(Vector3.Dot(hit.normal, ledgeHit.normal)) < 0.3f;
            endPos = hit.point;
            if (ledgeFound) {
                Vector3 ledgeHitDir = (ledgeHit.point - startPos).normalized;
                endPos = ledgeHit.point + new Vector3(0f, .65f, 0f) + (ledgeHitDir + ledgeHit.normal).normalized * ledgeSnapDistance;
            }

            player.movementHandler = new HookMovementHandler(player, startPos, endPos, ledgeFound ? ledgeHit.point: hit.point, ledgeFound);
            player.ability1Time = player.ability1Cooldown;
            AudioManager.instance.PlayOneShot(FMODEvents.instance.hookThrow, transform.position);
        }
    }
}
