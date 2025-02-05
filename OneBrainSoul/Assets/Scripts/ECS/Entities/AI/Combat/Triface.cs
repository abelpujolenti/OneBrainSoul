using System;
using System.Collections;
using System.Collections.Generic;
using AI.Combat.AbilityAoEColliders;
using AI.Combat.Contexts;
using AI.Combat.Enemy.Triface;
using AI.Combat.Position;
using AI.Combat.ScriptableObjects;
using ECS.Components.AI.Combat.Abilities;
using ECS.Components.AI.Navigation;
using Interfaces.AI.Combat;
using Managers;
using Player;
using UnityEngine;

namespace ECS.Entities.AI.Combat
{
    public class Triface : FreeMobilityEnemy<TrifaceContext, TrifaceAction>, INoProjectileAbility
    {
        [SerializeField] private TrifaceProperties _trifaceProperties;

        private RectangularAbilityComponent _slamAbility;
        private RectangularAbilityAoECollider _slamAoECollider;
        
        private void Start()
        {
            CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
            float radius = capsuleCollider.radius;
            
            EnemySetup(radius, _trifaceProperties);

            _utilityFunction = new TrifaceUtilityFunction();

            _context = new TrifaceContext(_trifaceProperties.totalHealth, radius, capsuleCollider.height,
                _trifaceProperties.sightMaximumDistance, transform, _slamAbility.GetCast());
            
            CombatManager.Instance.AddEnemy(this);

            _entityType = EntityType.TRIFACE;
        }

        protected override void InitiateDictionaries()
        {
            _actions = new Dictionary<TrifaceAction, Action>
            {
                { TrifaceAction.PATROL , Patrol },
                { TrifaceAction.LOOK_FOR_PLAYER , LookForPlayer },
                { TrifaceAction.GET_CLOSER_TO_PLAYER , GetCloserToPlayer },
                { TrifaceAction.ROTATE , Rotate },
                { TrifaceAction.SLAM , Slam }
            };
        }

        protected override void CreateAbilities()
        {
            _slamAbility = new RectangularAbilityComponent(_trifaceProperties.slamAbility);
            
            _slamAoECollider = InstantiateAbilityCollider<RectangularAbilityComponent, RectangularAbilityAoECollider>
                 (_slamAbility);
        }

        #region AI LOOP

        private void Update()
        {
            UpdateVisibleTargets();
            
            UpdateVectorToTarget();

            if (_context.IsCastingAnAbility())
            {
                return;
            }
                
            //LaunchRaycasts();
            
            CalculateBestAction();

            if (!_context.HasATarget())
            {
                return;
            }
                
            //TODO AGENT SLOTS
            AgentSlotPosition agentSlotPosition = CombatManager.Instance.RequestPlayer()
                .GetAgentSlotPosition(_context.GetVectorToTarget(), _context.GetRadius());

            if (agentSlotPosition == null)
            {
                return;
            }

            _agentSlot = agentSlotPosition.agentSlot;
            ECSNavigationManager.Instance.UpdateAStarDeviationVector(GetAgentID(), agentSlotPosition.deviationVector);
        }

        #endregion

        #region FSM

        protected override void UpdateVisibleTargets()
        {
            SetIsSeeingATarget(CombatManager.Instance.CanSeePlayer(transform.position, _context.GetSightMaximumDistance()));
        }

        private void Patrol()
        {
            ShowDebugMessages("Triface " + GetAgentID() + " Patrolling");
            
            //SetDestination(new VectorComponent(ReturnValidPositionInNavMesh()));

            //TODO TRIFACE PATROL
        }

        private void LookForPlayer()
        {
            ShowDebugMessages("Triface " + GetAgentID() + " Looking");
            PlayerCharacter playerCharacter = CombatManager.Instance.RequestPlayer();
            
            SetTargetRadius(playerCharacter.GetRadius());
            SetTargetHeight(playerCharacter.GetHeight());
            SetTargetTransform(playerCharacter.GetTransformComponent().GetTransform());
            
            SetDestination(playerCharacter.GetTransformComponent());
        }

        private void GetCloserToPlayer()
        {
            ShowDebugMessages("Triface " + GetAgentID() + " Getting Closer To Player");
            
            ContinueNavigation();
        }

        private void Slam()
        {
            ShowDebugMessages("Triface " + GetAgentID() + " Slaming");
            
            CastingAnAbility();
            
            StartCastingNoProjectileAbility<RectangularAbilityComponent, RectangularAbilityAoECollider>
                (_slamAbility, _slamAoECollider);
        }

        #endregion

        public override TrifaceContext GetContext()
        {
            return _context;
        }

        #region Attacks Managing

        public void StartCastingNoProjectileAbility<TAbilityComponent, TAbilityCollider>
            (TAbilityComponent abilityComponent, TAbilityCollider abilityCollider) 
                where TAbilityComponent : AbilityComponent
                where TAbilityCollider : AbilityAoECollider<TAbilityComponent>
        {
            if (abilityComponent.GetCast().IsOnCooldown())
            {
                NotCastingAnAbility();
                return;
            }
            
            abilityCollider.SetParent(transform);
            StartCoroutine(StartNoProjectileAbilityCastTimeCoroutine<TAbilityComponent, TAbilityCollider>
                (abilityComponent, abilityCollider));
        }

        public IEnumerator StartNoProjectileAbilityCastTimeCoroutine<TAbilityComponent, TAbilityCollider>
            (TAbilityComponent abilityComponent, TAbilityCollider abilityCollider) 
                where TAbilityCollider : AbilityAoECollider<TAbilityComponent> 
                where TAbilityComponent : AbilityComponent
        {
            abilityComponent.GetCast().StartCastTime();

            while (abilityComponent.GetCast().IsCasting())
            {
                abilityComponent.GetCast().DecreaseCurrentCastTime();
                yield return null;
            }
            
            AudioManager.instance.PlayOneShot(FMODEvents.instance.enemyAttack, transform.position);
            
            abilityCollider.Activate();

            Rotate();
            
            PutAbilityOnCooldown(abilityComponent);
        }

        protected override void PutAbilityOnCooldown(AbilityComponent abilityComponent)
        {
            NotCastingAnAbility();
            StartCoroutine(StartCooldownCoroutine(abilityComponent));
        }

        protected override IEnumerator StartCooldownCoroutine(AbilityComponent abilityComponent)
        {
            abilityComponent.GetCast().StartCooldown();

            while (abilityComponent.GetCast().IsOnCooldown())
            {
                abilityComponent.GetCast().DecreaseCooldown();
                yield return null;
            }
        }

        #endregion

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CombatManager.Instance.OnEnemyDefeated(this);
        }
    }
}