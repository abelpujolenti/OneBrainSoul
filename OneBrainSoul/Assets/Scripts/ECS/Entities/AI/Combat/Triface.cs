using System;
using System.Collections;
using System.Collections.Generic;
using AI.Combat.AbilityColliders;
using AI.Combat.Contexts;
using AI.Combat.Enemy.Triface;
using AI.Combat.ScriptableObjects;
using ECS.Components.AI.Combat;
using Managers;
using UnityEngine;

namespace ECS.Entities.AI.Combat
{
    public class Triface : FreeMobilityEnemy<TrifaceContext, TrifaceAction>
    {
        [SerializeField] private TrifaceSpecs _trifaceSpecs;
        
        //private 
        
        private void Start()
        {
            InitiateDictionaries();

            CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
            float radius = capsuleCollider.radius;
            
            //_context = new TrifaceContext()
            
            CombatManager.Instance.AddEnemy(this);
            
            EnemySetup(radius, _trifaceSpecs);
        }

        protected override void InitiateDictionaries()
        {
            _actions = new Dictionary<TrifaceAction, Action>
            {
                { TrifaceAction.PATROL , Patrol },
                { TrifaceAction.LOOK_FOR_PLAYER , LookForPlayer },
                { TrifaceAction.GET_CLOSER_TO_PLAYER , GetCloserToPlayer },
                { TrifaceAction.ROTATE , Rotate },
                { TrifaceAction.SLAM , Slam },
            };
        }

        #region AI LOOP

        private void Update()
        {
            UpdateVectorToTarget();

            if (_context.IsAttacking())
            {
                return;
            }
                
            //LaunchRaycasts();
            
            CalculateBestAction();

            if (!_context.HasATarget())
            {
                return;
            }
                
            //TODO
            /*AgentSlotPosition agentSlotPosition = CombatManager.Instance.RequestPlayer()
                .GetAgentSlotPosition(_context.GetVectorToRival(), _context.GetRadius());

            if (agentSlotPosition == null)
            {
                yield return null;
                continue;
            }

            _agentSlot = agentSlotPosition.agentSlot;
            ECSNavigationManager.Instance.UpdateAStarDeviationVector(GetAgentID(), agentSlotPosition.deviationVector);*/
        }

        #endregion

        #region FSM

        private void Patrol()
        {
        }

        private void LookForPlayer()
        {
        }

        private void GetCloserToPlayer()
        {
        }

        private void Slam()
        {
        }

        #endregion

        public override TrifaceContext GetContext()
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