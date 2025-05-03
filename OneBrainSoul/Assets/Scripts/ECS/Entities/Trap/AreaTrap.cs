using System.Collections;
using AI.Combat.AbilitySpecs;
using AI.Combat.ScriptableObjects.Traps;
using Interfaces.AI.Combat;
using Managers;
using UnityEngine;

namespace ECS.Entities.Trap
{
    public class AreaTrap : MonoBehaviour
    {
        [SerializeField] private AreaTrapProperties _areaTrapProperties;

        private IAreaAbility _areaAbility;

        private void Start()
        {
            CreateAbility();
            
            _areaAbility.GetCast().ResetCastTime();
        }

        private void CreateAbility()
        {
            _areaAbility = AbilityManager.Instance.ReturnAreaAbility(_areaTrapProperties.areaAbilityProperties, transform);
        }

        private void Update()
        {
            if (_areaAbility.GetCast().IsOnCooldown())
            {
                return;
            }

            StartCoroutine(StartCastingAreaAbility());
        }

        private IEnumerator StartCastingAreaAbility()
        {
            AbilityCast abilityCast = _areaAbility.GetCast();
            
            abilityCast.StartCastTime();

            while (abilityCast.IsCasting())
            {
                abilityCast.DecreaseCurrentCastTime();

                yield return null;
            }
            
            AudioManager.Instance.PlayOneShot(_areaTrapProperties.areaAbilityProperties.executeAbilitySound, transform.position);
            
            _areaAbility.Activate();

            StartCoroutine(StartCooldownCoroutine(_areaAbility.GetCast()));
        }

        private IEnumerator StartCooldownCoroutine(AbilityCast abilityCast)
        {
            abilityCast.StartCooldown();

            while (abilityCast.IsOnCooldown())
            {
                yield return null;
                abilityCast.DecreaseCooldown();
            }
        }
    }
}