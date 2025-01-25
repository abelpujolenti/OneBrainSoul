using System;
using System.Collections;
using System.Collections.Generic;
using AI.Combat.AbilityColliders;
using AI.Combat.Contexts;
using AI.Combat.Enemy.LongArms;
using AI.Combat.ScriptableObjects;
using ECS.Components.AI.Combat;
using Managers;
using UnityEngine;

namespace ECS.Entities.AI.Combat
{
    public class LongArms : TeleportMobilityEnemy<LongArmsContext, LongArmsAction>
    {
        [SerializeField] private LongArmsSpecs _longArmsSpecs;
        
        private void Start()
        {
            InitiateDictionaries();

            CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
            float radius = capsuleCollider.radius;
            
            //_context = new TrifaceContext()
            
            CombatManager.Instance.AddEnemy(this);
            
            EnemySetup(radius, _longArmsSpecs);
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

        #region FSM

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

        protected override void StartCastingAbility(AttackComponent attackComponent)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerator StartAbilityCastTimeCoroutine(AttackComponent attackComponent, AbilityCollider abilityCollider)
        {
            throw new NotImplementedException();
        }

        protected override void PutAbilityOnCooldown(AttackComponent attackComponent)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerator StartCooldownCoroutine(AttackComponent attackComponent)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override void OnReceiveDamage(DamageComponent damageComponent)
        {
            throw new NotImplementedException();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CombatManager.Instance.OnEnemyDefeated(this);
        }
    }
}