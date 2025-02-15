using System;
using System.Collections;
using System.Collections.Generic;
using AI.Combat.AbilitySpecs;
using AI.Combat.Contexts;
using AI.Combat.Enemy.Triface;
using AI.Combat.Position;
using AI.Combat.ScriptableObjects;
using Interfaces.AI.Combat;
using Managers;
using UnityEngine;

namespace ECS.Entities.AI.Combat
{
    public class Triface : FreeMobilityEnemy<TrifaceContext, TrifaceAction>
    {
        [SerializeField] private TrifaceProperties _trifaceProperties;

        private IAreaAbility _slamAbility;
        private HashSet<uint> _visibleTargetsForSlamAbility;
        private Func<bool> _cancelSlamFunc = () => false;

        private float _rotationSpeedWhenCastingSlam;
        
        private void Start()
        {
            CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
            float radius = capsuleCollider.radius;
            
            EnemySetup(radius, _trifaceProperties, EntityType.TRIFACE);

            _utilityFunction = new TrifaceUtilityFunction();

            _normalRotationSpeed = _trifaceProperties.normalRotationSpeed;
            _rotationSpeedWhenCastingSlam = _trifaceProperties.rotationSpeedWhileCastingSlam;

            _context = new TrifaceContext(_trifaceProperties.totalHealth, radius, capsuleCollider.height,
                _trifaceProperties.sightMaximumDistance, _trifaceProperties.fov, transform, _slamAbility.GetCast());
            
            CombatManager.Instance.AddEnemy(this);
        }

        protected override void InitiateDictionaries()
        {
            _actions = new Dictionary<TrifaceAction, Action>
            {
                { TrifaceAction.PATROL , Patrol },
                { TrifaceAction.ACQUIRE_NEW_TARGET_FOR_SLAM , AcquireNewTargetForSlam },
                { TrifaceAction.GET_CLOSER_TO_TARGET_OF_SLAM , GetCloserToTargetForSlam },
                { TrifaceAction.ROTATE , RotateInSitu },
                { TrifaceAction.SLAM , Slam }
            };
        }

        protected override void CreateAbilities()
        {
            _slamAbility = AbilityManager.Instance.ReturnAreaAbility(_trifaceProperties.slamAbilityProperties,
                transform);

            if (!_slamAbility.GetCast().canCancelCast)
            {
                return;
            }

            _cancelSlamFunc = () =>
            {
                AbilityCast abilityCast = _slamAbility.GetCast();
                Vector3 vectorToTarget = _context.GetSlamTargetContext().GetVectorToTarget();

                return Vector3.Angle(transform.forward, vectorToTarget) > abilityCast.maximumAngleToCancelCast ||
                       vectorToTarget.sqrMagnitude > abilityCast.maximumRangeToCast * abilityCast.maximumRangeToCast;
            };
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

            if (!_context.HasATargetForSlam())
            {
                return;
            }
                
            //TODO AGENT SLOTS
            AgentSlotPosition agentSlotPosition = CombatManager.Instance.ReturnAgentEntity(_slamAbility.GetTargetId())
                .GetAgentSlotPosition(_context.GetSlamTargetContext().GetVectorToTarget(), _context.GetRadius());

            if (agentSlotPosition == null)
            {
                return;
            }

            _agentSlot = agentSlotPosition.agentSlot;
            ECSNavigationManager.Instance.UpdateAStarDeviationVector(GetAgentID(), agentSlotPosition.deviationVector);
        }

        protected override void UpdateVisibleTargets()
        {
            Transform ownTransform = transform;
            
            _visibleTargetsForSlamAbility = CombatManager.Instance.ReturnVisibleTargets(
                _trifaceProperties.slamAbilityProperties.abilityTarget, ownTransform.position, 
                _context.GetSightMaximumDistance(), ownTransform.forward, _context.GetFov());
            
            _context.SetIsSeeingATargetForSlam(_visibleTargetsForSlamAbility.Count != 0);
        }

        private void UpdateVectorToTarget()
        {
            if (!_context.HasATargetForSlam())
            {
                return;
            }

            Vector3 targetPosition = _context.GetSlamTargetContext().GetTargetTransform().position;
            Vector3 agentPosition = transform.position;
            
            targetPosition.y -= _context.GetSlamTargetContext().GetTargetHeight() / 2;
            agentPosition.y -= _context.GetHeight() / 2;
            
            _context.GetSlamTargetContext().SetVectorToTarget(targetPosition - agentPosition);
        }

        #endregion

        #region FSM

        private void Patrol()
        {
            ShowDebugMessages("Triface " + GetAgentID() + " Patrolling");
            
            //SetDestination(new VectorComponent(ReturnValidPositionInNavMesh()));

            //TODO TRIFACE PATROL
        }

        private void AcquireNewTargetForSlam()
        {
            ShowDebugMessages("Triface " + GetAgentID() + " Acquiring New Target For Slam");

            Vector3 position = transform.position;
            position.y -= GetHeight() / 2;

            AgentEntity target = CombatManager.Instance.ReturnClosestAgentEntity(position, _visibleTargetsForSlamAbility);
            
            _context.SetSlamTarget(target);
            
            _slamAbility.SetTargetId(target.GetAgentID());
            
            SetDestination(target.GetTransformComponent());
        }

        private void GetCloserToTargetForSlam()
        {
            ShowDebugMessages("Triface " + GetAgentID() + " Getting Closer To Target For Slam");
            
            ContinueNavigation();
        }

        private void Slam()
        {
            ShowDebugMessages("Triface " + GetAgentID() + " Slaming");
            
            StartCastingSlam(_slamAbility);
        }

        #endregion

        public override TrifaceContext GetContext()
        {
            return _context;
        }

        #region Abilities Managing

        #region Slam

        private void StartCastingSlam(IAreaAbility areaAbility) 
        {
            CastingAnAbility();

            _currentRotationSpeed = _rotationSpeedWhenCastingSlam;
            
            StartCoroutine(StartSlamCastTimeCoroutine(areaAbility));
        }

        private IEnumerator StartSlamCastTimeCoroutine(IAreaAbility areaAbility)
        {
            AbilityCast abilityCast = areaAbility.GetCast(); 
            
            abilityCast.StartCastTime();

            while (abilityCast.IsCasting())
            {
                abilityCast.DecreaseCurrentCastTime();
                
                Rotate();
                
                if (_cancelSlamFunc())
                {
                    abilityCast.ResetCastTime();
                    NotCastingAnAbility();
                    _currentRotationSpeed = _normalRotationSpeed;
                    yield break;
                }
                yield return null;
            }
            
            _currentRotationSpeed = _normalRotationSpeed;
            
            AudioManager.instance.PlayOneShot(FMODEvents.instance.enemyAttack, transform.position);
            
            areaAbility.Activate();
            
            StartCoroutine(StartCooldownCoroutine(areaAbility.GetCast()));
        }

        #endregion

        #endregion

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CombatManager.Instance.OnEnemyDefeated(this);
        }
        
        
        /////////////////////////DEBUG
        
        [SerializeField] private bool _showDetectionAreaOfSlam;
        [SerializeField] private Color _colorOfDetectionAreaOfSlam;

        private void OnDrawGizmos()
        {
            
            Vector3 origin = transform.position;
            int segments = 20;

            if (_showFov)
            {
                DrawCone(_fovColor, _context.GetFov(), 0, _context.GetSightMaximumDistance(), origin, transform.forward, segments);
            }
            
            if (_showDetectionAreaOfSlam)
            {
                DrawAbilityCone(_colorOfDetectionAreaOfSlam, _context.HasATargetForSlam(), _slamAbility.GetCast(), origin,
                    _context.GetDirectionOfSlamDetection(), segments);
            }
        }
    }
}