using Managers;
using Unity.VisualScripting;
using UnityEngine;

public class HookAbility : MonoBehaviour
{
    [SerializeField] HookUI hookUI;
    [SerializeField] Material hookshotChainMaterial;
    [Range(1f, 1000f)]
    [SerializeField] float range = 100f;
    [Range(1, 7)]
    [SerializeField] int maxHookCharges = 3;
    [Range(0f, 3f)]
    [SerializeField] float radius = .5f;
    [Range(1f, 10f)]
    [SerializeField] float ledgeSnapLeniency = 4f;
    [Range(0f, 1f)]
    [SerializeField] float ledgeSnapNormalThreshold = .5f;
    [Range(0f, 3f)]
    [SerializeField] float ledgeSnapDistance = 1f;

    [Range(0.5f, 20f)]
    [SerializeField] private float outOfCombatRechargeDuration = 0.5f;
    [Range(0.5f, 20f)]
    [SerializeField] private float inCombatRechargeDuration = 2f;

    PlayerCharacterController player;
    RaycastHit hit;

    int hookCharges = 0;
    float rechargeTime = 0f;

    private void Start()
    {
        player = GetComponent<PlayerCharacterController>();
        var line = player.AddComponent<LineRenderer>();
        line.startWidth = 0.4f;
        line.endWidth = 0.3f;
        line.material = hookshotChainMaterial;
        hookCharges = maxHookCharges;
        hookUI.SetMaxCharges(maxHookCharges);
    }

    private void Update()
    {
        if (player.movementHandler is HookMovementHandler)
        {
            (player.movementHandler as HookMovementHandler).VisualUpdate(player);
        }

        Vector3 startPos, endPos;
        Vector3 dir = player.cam.transform.forward;
        startPos = player.transform.position + new Vector3(0f, .5f, 0f);
        bool landed = Physics.SphereCast(startPos + dir * 2f, radius, dir, out hit, range, GameManager.Instance.GetRaycastLayersWithoutAlly(), QueryTriggerInteraction.Ignore);

        if (landed)
        {
            if (player.ability2Time == 0f)
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


        if (hookCharges > 0 && player.ability2Input && player.ability2Time == 0f && landed)
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
            player.ability2Time = player.ability2Cooldown;

            AudioManager.instance.PlayOneShot(FMODEvents.instance.hookThrow, transform.position);

            hookCharges--;
        }

        if (player.inCombat && rechargeTime > inCombatRechargeDuration || !player.inCombat && rechargeTime > outOfCombatRechargeDuration)
        {
            hookCharges++;
            rechargeTime = 0f;
        }

        hookUI.UpdateUI(hookCharges, rechargeTime, player.inCombat ? inCombatRechargeDuration : outOfCombatRechargeDuration);

        if (hookCharges < maxHookCharges)
        {
            rechargeTime += Time.deltaTime;
        }
    }
}
