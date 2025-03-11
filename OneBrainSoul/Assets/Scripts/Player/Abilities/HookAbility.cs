using Managers;
using Player.Movement;
using UnityEngine;

namespace Player.Abilities
{
    public class HookAbility : MonoBehaviour
    {
        [SerializeField] Material _hookShotChainMaterial;
        [Range(1f, 1000f)]
        [SerializeField] private float _range;
        [Range(0f, 3f)]
        [SerializeField] private float _radius;
        [Range(1f, 10f)]
        [SerializeField] private float _ledgeSnapLeniency;
        [Range(0f, 1f)]
        [SerializeField] private float _ledgeSnapNormalThreshold;
        [Range(0f, 3f)]
        [SerializeField] private float _ledgeSnapDistance;
        [Range(0f,1f)]
        [SerializeField] private float _smashThreshold;
        [Range(0f, 1f)]
        [SerializeField] private float _smashNormalThreshold;

        private RaycastHit _hit;


        public void Setup(LineRenderer lineRenderer)
        {
            lineRenderer.startWidth = 0.4f;
            lineRenderer.endWidth = 0.3f;
            lineRenderer.material = _hookShotChainMaterial;
        }
        
        //TODO ADRI PASA-HO PER ONGUI(), I LES COSES QUE FACI AQUEST UPDATE MENTRES ESTA EN HOOK PASA-HO PER UNA COROUTINE 
        public void FakeUpdate(PlayerCharacterController player)
        {
            if (player.GetMovementHandler() is HookMovementHandler)
            {
                (player.GetMovementHandler() as HookMovementHandler).VisualUpdate(player);
            }

            Vector3 startPos, endPos;
            Vector3 dir = player.GetCamera().transform.forward;
            startPos = player.transform.position + new Vector3(0f, .5f, 0f);
            bool landed = Physics.SphereCast(startPos + dir * 2f, _radius, dir, out _hit, _range,
                GameManager.Instance.GetRaycastLayersWithoutAlly(), QueryTriggerInteraction.Ignore);

            if (landed)
            {
                if (player.GetAbility2Time() == 0f)
                {
                    player.SetCrosshairColor(new Color(1f, .1f, 1f));
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

            if (player.GetCharges() > 0 && player.GetAbility2Input() && player.GetAbility2Time() == 0f && landed)
            {
                RaycastHit ledgeHit;
                _ledgeSnapLeniency = 4f;
                Vector3 dirNoY = new Vector3(dir.x, 0f, dir.z).normalized;
                
                bool ledgeFound = Physics.SphereCast(_hit.point + dirNoY * .15f + _ledgeSnapLeniency * Vector3.up, .5f,
                    Vector3.down, out ledgeHit, _ledgeSnapLeniency, GameManager.Instance.GetRaycastLayersWithoutAlly(),
                    QueryTriggerInteraction.Ignore);
                
                ledgeFound &= ledgeHit.point != Vector3.zero;
                ledgeFound &= Vector3.Dot(Vector3.up, ledgeHit.normal) > _ledgeSnapNormalThreshold;
                ledgeFound &= Mathf.Abs(Vector3.Dot(_hit.normal, ledgeHit.normal)) < 0.3f;
                endPos = _hit.point;
                if (ledgeFound) {
                    Vector3 ledgeHitDir = (ledgeHit.point - startPos).normalized;
                    endPos = ledgeHit.point + new Vector3(0f, .65f, 0f) + (ledgeHitDir + ledgeHit.normal).normalized * _ledgeSnapDistance;
                }

                bool smash = player._isChargeUnlocked && dir.y <= -_smashThreshold && _hit.normal.y >= _ledgeSnapNormalThreshold;

                player.ChangeMovementHandlerToHook(startPos, endPos, ledgeFound ? ledgeHit.point: _hit.point, ledgeFound, smash);
                player.ResetAbility2Cooldown();

                AudioManager.instance.PlayOneShot(FMODEvents.instance.hookThrow, transform.position);

                player.ConsumeCharge();
            }
        }
    }
}
