using System.Collections;
using AI.Combat.AbilitySpecs;
using AI.Combat.ScriptableObjects.Traps;
using Interfaces.AI.Combat;
using Managers;
using UnityEngine;

namespace ECS.Entities.Trap
{
    public class ProjectileTrap : MonoBehaviour
    {
        [SerializeField] private ProjectileTrapProperties _projectileTrapProperties;

        [SerializeField] private Vector3 _projectileDirection;

        private IProjectileAbility _projectileAbility;

        private void Start()
        {
            CreateAbility();
            
            _projectileAbility.GetCast().ResetCastTime();
        }

        private void CreateAbility()
        {
            _projectileAbility = AbilityManager.Instance.ReturnProjectileAbility(_projectileTrapProperties.projectileAbilityProperties, 
                transform);
        }

        private void Update()
        {
            if (_projectileAbility.GetCast().IsOnCooldown())
            {
                return;
            }

            StartCoroutine(StartCastingProjectileAbility());
        }

        private IEnumerator StartCastingProjectileAbility()
        {
            _projectileAbility.Activate();
            
            AbilityCast abilityCast = _projectileAbility.GetCast();
            
            abilityCast.StartCastTime();

            while (abilityCast.IsCasting())
            {
                abilityCast.DecreaseCurrentCastTime();

                yield return null;
            }
            
            AudioManager.Instance.PlayOneShot(_projectileTrapProperties.projectileAbilityProperties.executeAbilitySound, transform.position);
            
            _projectileAbility.FIREEEEEEEEEEEEEE(Quaternion.LookRotation(transform.forward, Vector3.up) * _projectileDirection);

            StartCoroutine(StartCooldownCoroutine(_projectileAbility.GetCast()));
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