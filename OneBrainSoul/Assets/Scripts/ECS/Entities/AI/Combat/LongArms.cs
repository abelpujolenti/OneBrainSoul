using System;
using System.Collections;
using System.Collections.Generic;
using AI.Combat.AbilityAoEColliders;
using AI.Combat.Contexts;
using AI.Combat.Enemy.LongArms;
using AI.Combat.ScriptableObjects;
using ECS.Components.AI.Combat.Abilities;
using Interfaces.AI.Combat;
using Managers;
using UnityEngine;
using UnityEngine.Serialization;

namespace ECS.Entities.AI.Combat
{
    public class LongArms : TeleportMobilityEnemy<LongArmsContext, LongArmsAction>, INoProjectileAbility, IProjectileAbility
    {
        [FormerlySerializedAs("_longArmsSpecs")] [SerializeField] private LongArmsProperties longArmsProperties;
        
        private void Start()
        {
            InitiateDictionaries();

            CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
            float radius = capsuleCollider.radius;

            _context = new LongArmsContext(longArmsProperties.totalHealth, radius, capsuleCollider.height,
                longArmsProperties.sightMaximumDistance, transform);
            
            CombatManager.Instance.AddEnemy(this);

            _entityType = EntityType.LONG_ARMS;
            
            EnemySetup(radius, longArmsProperties);
        }

        #region AI LOOP

        private void Update()
        {
            throw new NotImplementedException();
        }

        #endregion

        protected override void InitiateDictionaries()
        {
            _actions = new Dictionary<LongArmsAction, Action>
            {
                { LongArmsAction.LOOKING , Looking},
                { LongArmsAction.THROW_ROCK , ThrowRock},
                { LongArmsAction.CLAP_ABOVE , ClapAbove},
                { LongArmsAction.FLEE , Flee}
            };
        }
        
        protected override void CreateAbilities()
        {
            //InstantiateAbility(_longArmsSpecs.slamAbility);
        }

        #region FSM

        protected override void UpdateVisibleTargets()
        {
            
        }

        private void Looking()
        {
        }

        private void ThrowRock()
        {
        }

        private void ClapAbove()
        {
        }

        private void Flee()
        {
        }

        #endregion

        public override LongArmsContext GetContext()
        {
            return _context;
        }

        #region Attacks Managing

        public void StartCastingNoProjectileAbility<TAbilityComponent, TAbilityCollider>(TAbilityComponent abilityComponent,
            TAbilityCollider abilityCollider) where TAbilityComponent : AbilityComponent where TAbilityCollider : AbilityAoECollider<TAbilityComponent>
        {
            if (abilityComponent.GetCast().IsOnCooldown())
            {
                NotCastingAnAbility();
                return;
            }
            
            abilityCollider.SetParent(transform);
            StartCoroutine(StartNoProjectileAbilityCastTimeCoroutine(abilityComponent, abilityCollider));
        }

        public IEnumerator StartNoProjectileAbilityCastTimeCoroutine<TAbilityComponent, TAbilityCollider>(
            TAbilityComponent abilityComponent, TAbilityCollider abilityCollider) where TAbilityComponent : AbilityComponent where TAbilityCollider : AbilityAoECollider<TAbilityComponent>
        {
            abilityComponent.GetCast().StartCastTime();

            while (abilityComponent.GetCast().IsCasting())
            {
                abilityComponent.GetCast().DecreaseCurrentCastTime();
                yield return null;
            }
            
            abilityCollider.Activate();
            
            PutAbilityOnCooldown(abilityComponent);
        }

        protected override void PutAbilityOnCooldown(AbilityComponent abilityComponent)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerator StartCooldownCoroutine(AbilityComponent abilityComponent)
        {
            throw new NotImplementedException();
        }

        #endregion

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CombatManager.Instance.OnEnemyDefeated(this);
        }
    }
}