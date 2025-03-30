using System.Collections;
using AI.Combat.AbilitySpecs;
using AI.Combat.ScriptableObjects;
using Interfaces.AI.Combat;
using Managers;
using UnityEngine;

namespace ECS.Entities
{
    public class Trap : MonoBehaviour
    {
        [SerializeField] private TrapProperties _trapProperties;

        private IAreaAbility _trapAbility;

        private void Start()
        {
            CreateAbility();
            
            _trapAbility.GetCast().ResetCastTime();
        }

        private void CreateAbility()
        {
            _trapAbility = AbilityManager.Instance.ReturnAreaAbility(_trapProperties.trapAbilityProperties, transform);
        }

        private void Update()
        {
            if (_trapAbility.GetCast().IsOnCooldown())
            {
                return;
            }

            StartCoroutine(StartCastingAbility(_trapAbility));
        }

        private IEnumerator StartCastingAbility(IAreaAbility areaAbility)
        {
            AbilityCast abilityCast = areaAbility.GetCast();
            
            abilityCast.StartCastTime();

            while (abilityCast.IsCasting())
            {
                abilityCast.DecreaseCurrentCastTime();

                yield return null;
            }
            
            AudioManager.instance.PlayOneShot(_trapProperties.trapAbilityProperties.executeAbilitySound, transform.position);
            
            areaAbility.Activate();

            StartCoroutine(StartCooldownCoroutine(areaAbility.GetCast()));
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