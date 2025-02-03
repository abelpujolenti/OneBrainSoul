using Managers;
using Player.Movement;
using UnityEngine;

namespace Player.Abilities
{
    public class HookAbility : MonoBehaviour
    {
        [SerializeField] HookUI _hookUI;
        [SerializeField] Material _hookShotChainMaterial;
        [Range(1f, 1000f)]
        [SerializeField] private float _range;
        [Range(1, 7)]
        [SerializeField] private int _maxHookCharges;
        [Range(0f, 3f)]
        [SerializeField] private float _radius;
        [Range(1f, 10f)]
        [SerializeField] private float _ledgeSnapLeniency;
        [Range(0f, 1f)]
        [SerializeField] private float _ledgeSnapNormalThreshold;
        [Range(0f, 3f)]
        [SerializeField] private float _ledgeSnapDistance;

        [Range(0.5f, 20f)]
        [SerializeField] private float _outOfCombatRechargeDuration;
        [Range(0.5f, 20f)]
        [SerializeField] private float _inCombatRechargeDuration;

        private RaycastHit _hit;

        private int _hookCharges;
        private float _rechargeTime;

        public void Setup(LineRenderer lineRenderer)
        {
            lineRenderer.startWidth = 0.4f;
            lineRenderer.endWidth = 0.3f;
            lineRenderer.material = _hookShotChainMaterial;
            _hookCharges = _maxHookCharges;
            _hookUI.SetMaxCharges(_maxHookCharges);
        }
        
        //TODO ADRI PASA-HO PER ONGUI(), I LES COSES QUE FACI AQUEST UPDATE MENTRES ESTA EN HOOK PASA-HO PER UNA COROUTINE 
        public void FakeUpdate(PlayerCharacterController playerCharacterController)
        {
            if (playerCharacterController.GetMovementHandler() is HookMovementHandler)
            {
                (playerCharacterController.GetMovementHandler() as HookMovementHandler).VisualUpdate(playerCharacterController);
            }

            Vector3 startPos, endPos;
            Vector3 dir = playerCharacterController.GetCamera().transform.forward;
            startPos = playerCharacterController.transform.position + new Vector3(0f, .5f, 0f);
            bool landed = Physics.SphereCast(startPos + dir * 2f, _radius, dir, out _hit, _range,
                GameManager.Instance.GetRaycastLayersWithoutAlly(), QueryTriggerInteraction.Ignore);

            if (landed)
            {
                if (playerCharacterController.GetAbility2Time() == 0f)
                {
                    playerCharacterController.SetCrosshairColor(new Color(.9f, .9f, .1f));
                }
                else
                {
                    playerCharacterController.SetCrosshairColor(new Color(.1f, .9f, .9f));
                }
            }
            else
            {
                playerCharacterController.SetCrosshairColor(new Color(1f, 1f, 1f));
            }


            if (_hookCharges > 0 && playerCharacterController.GetAbility2Input() && playerCharacterController.GetAbility2Time() == 0f && landed)
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

                playerCharacterController.ChangeMovementHandlerToHook(startPos, endPos, ledgeFound ? ledgeHit.point: _hit.point, ledgeFound);
                playerCharacterController.ResetAbility2Cooldown();

                AudioManager.instance.PlayOneShot(FMODEvents.instance.hookThrow, transform.position);

                _hookCharges--;
            }

            if (playerCharacterController.IsInCombat() && 
                _rechargeTime > _inCombatRechargeDuration || 
                !playerCharacterController.IsInCombat() && 
                _rechargeTime > _outOfCombatRechargeDuration)
            {
                _hookCharges++;
                _rechargeTime = 0f;
            }

            _hookUI.UpdateUI(_hookCharges, _rechargeTime, 
                playerCharacterController.IsInCombat() ? _inCombatRechargeDuration : _outOfCombatRechargeDuration);

            if (_hookCharges < _maxHookCharges)
            {
                _rechargeTime += Time.deltaTime;
            }
        }
    }
}
