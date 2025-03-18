using System;
using System.Collections;
using System.Collections.Generic;
using AI.Combat.AbilitySpecs;
using AI.Combat.Area;
using AI.Combat.Contexts;
using AI.Combat.Enemy.Triface;
using AI.Combat.Position;
using AI.Combat.ScriptableObjects;
using ECS.Components.AI.Navigation;
using Interfaces.AI.Combat;
using Managers;
using UnityEngine;

namespace ECS.Entities.AI.Combat
{
    public class Triface : FreeMobilityEnemy<TrifaceContext, TrifaceAction>
    {
        [SerializeField] private TrifaceProperties _trifaceProperties;

        [SerializeField] private bool _itGoesOnAutomatic;

        private GameObject _endEffector;

        [SerializeField] private AbilityDetectionArea _slamAbilityDetectionArea;
        private IAreaAbility _slamAbility;
        private HashSet<uint> _visibleTargetsForSlamAbility;
        private Func<bool> _cancelSlamFunc = () => false;

        private float _rotationSpeedWhenCastingSlam;

        private Action _update = () => { };

        private void Start()
        {
            CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
            float radius = capsuleCollider.radius;
            
            EnemySetup(radius, _trifaceProperties, EntityType.TRIFACE, _trifaceProperties.slamAbilityProperties.abilityTarget);

            _utilityFunction = new TrifaceUtilityFunction();

            _rotationSpeedWhenCastingSlam = _trifaceProperties.rotationSpeedWhileCastingSlam;

            _context = new TrifaceContext(_trifaceProperties.totalHealth, radius, capsuleCollider.height, _headTransform, 
                _bodyTransform, _slamAbility.GetCast());
            
            GetNavMeshAgentComponent().GetAStarPath().SetOnReachDestination(_context.SetHasReachedDestination);
            
            _slamAbilityDetectionArea.Setup(_trifaceProperties.slamAbilityProperties.abilityTarget, 
                _context.AddTargetInsideSlamDetectionArea, _context.RemoveTargetInsideSlamDetectionArea);
            
            CombatManager.Instance.AddEnemy(this);

            _startPosition = transform.position;

            if (_itGoesOnAutomatic)
            {
                _slamAbility.GetCast().ResetCastTime();
                _update = () => OnAutomatic();
                return;
            }

            _update = () => AILoop();
        }

        protected override void InitiateDictionaries()
        {
            _actions = new Dictionary<TrifaceAction, Action>
            {
                { TrifaceAction.CONTINUE_NAVIGATION , ContinueNavigation },
                { TrifaceAction.ROTATE , RotateInSitu },
                { TrifaceAction.PATROL , Patrol },
                { TrifaceAction.INVESTIGATE_AREA , InvestigateArea },
                { TrifaceAction.GO_TO_CLOSEST_SIGHTED_TARGET , GoToClosestSightedTarget },
                { TrifaceAction.ACQUIRE_NEW_TARGET_FOR_SLAM , AcquireNewTargetForSlam },
                { TrifaceAction.SLAM , Slam }
            };
        }

        protected override void CreateAbilities()
        {
            base.CreateAbilities();
            
            _slamAbility = AbilityManager.Instance.ReturnAreaAbility(_trifaceProperties.slamAbilityProperties,
                transform);

            if (!_slamAbility.GetCast().canCancelCast)
            {
                return;
            }

            _cancelSlamFunc = () => _context.IsSlamTargetInsideDetectionArea();
        }

        #region AI LOOP

        private void Update()
        {
            _update();
        }

        private void OnAutomatic()
        {
            if (_slamAbility.GetCast().IsOnCooldown())
            {
                return;
            }
                
            Slam();
        }

        private void AILoop()
        {
            ShowDebugMessages("Holi");
            
            UpdateSightedTargetsInsideCombatArea();
            
            UpdateVisibleTargets();
            
            UpdateVectorToTargets();

            if (_context.IsFSMBlocked())
            {
                return;
            }
            
            //RotateBody();
            
            //RotateHead();
                
            //LaunchRaycasts();
            
            CalculateBestAction();

            if (!_context.HasATargetForSlam())
            {
                return;
            }
            
            Vector3 vectorToTarget = _context.GetSlamTargetContext().GetVectorToTarget();
                
            //TODO AQUI AGENT SLOTS WHEN NEAR
            AgentSlotPosition agentSlotPosition = CombatManager.Instance.ReturnAgentEntity(_slamAbility.GetTargetId())
                .GetAgentSlotPosition(vectorToTarget, _context.GetRadius());

            if (agentSlotPosition == null)
            {
                StopNavigation();
                SetDirectionToRotateBody(vectorToTarget);
                ECSNavigationManager.Instance.UpdateAStarDeviationVector(GetAgentID(), -vectorToTarget);
                return;
            }
            
            _agentSlot = agentSlotPosition.agentSlot;
            ECSNavigationManager.Instance.UpdateAStarDeviationVector(GetAgentID(), agentSlotPosition.deviationVector);
        }

        
        //TODO ERASE
        private Vector3 _startPosition;
        //

        protected override void UpdateSightedTargetsInsideCombatArea()
        {
            base.UpdateSightedTargetsInsideCombatArea();

            RemoveSlamTargetIfWasLost(_slamAbility.GetTargetId());
        }

        private void RemoveSlamTargetIfWasLost(uint targetIdToCheck)
        {
            if (!_context.HasATargetForSlam() || _targetsSightedInsideCombatArea.Contains(targetIdToCheck))
            {
                return;
            }
            
            ShowDebugMessages("Triface " + GetAgentID() + " Losing Target");
            
            _context.LoseSlamTarget();
            
            SetDestination(new VectorComponent(_startPosition));
        }

        protected override void RemoveATargetIfWasLost(uint targetIdToCheck)
        {
            RemoveSlamTargetIfWasLost(targetIdToCheck);
        }

        protected override void UpdateVisibleTargets()
        {
            Transform ownTransform = transform;
            
            _visibleTargetsForSlamAbility = CombatManager.Instance.ReturnVisibleTargets(
                _trifaceProperties.slamAbilityProperties.abilityTarget, ownTransform.position, 
                _targetsInsideVisionArea, _areaNumber);
            
            _context.SetIsSeeingATargetForSlam(_visibleTargetsForSlamAbility.Count != 0);
        }

        private void UpdateVectorToTargets()
        {
            if (!_context.HasATargetForSlam())
            {
                return;
            }

            Vector3 targetPosition;
            Vector3 targetVelocity;

            {
                AgentEntity target = CombatManager.Instance.ReturnAgentEntity(_slamAbility.GetTargetId());
                targetPosition = target.GetTransformComponent().GetPosition();
                targetVelocity = target.GetVelocity();
            }

            _context.GetSlamTargetContext().SetTargetState(targetPosition, targetVelocity);
            
            Vector3 agentPosition = transform.position;
            
            targetPosition.y -= _context.GetSlamTargetContext().GetTargetHeight() / 2;
            agentPosition.y -= _context.GetHeight() / 2;

            Vector3 vectorToTarget = targetPosition - agentPosition;
            
            _context.GetSlamTargetContext().SetVectorToTarget(vectorToTarget);
            
            SetDirectionToRotateBody(vectorToTarget);
        }

        #endregion

        #region FSM

        private void AcquireNewTargetForSlam()
        {
            ShowDebugMessages("Triface " + GetAgentID() + " Acquiring New Target For Slam");

            Vector3 position = transform.position;
            position.y -= GetHeight() / 2;

            AgentEntity target = CombatManager.Instance.ReturnClosestAgentEntity(position, _visibleTargetsForSlamAbility);
            
            _context.SetSlamTargetProperties(target.GetAgentID(), target.GetRadius(), target.GetHeight());
            
            _slamAbility.SetTargetId(target.GetAgentID());
            
            SetDestination(target.GetTransformComponent());
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
            BlockFSM();
            
            StopNavigation();

            _bodyCurrentRotationSpeed = _rotationSpeedWhenCastingSlam;
            
            SetDirectionToRotateBody(_context.GetSlamTargetContext().GetVectorToTarget());
            
            StartCoroutine(StartSlamCastTimeCoroutine(areaAbility));
        }

        private IEnumerator StartSlamCastTimeCoroutine(IAreaAbility areaAbility)
        {
            AbilityCast abilityCast = areaAbility.GetCast(); 
            
            abilityCast.StartCastTime();

            while (abilityCast.IsCasting())
            {
                abilityCast.DecreaseCurrentCastTime();
                
                RotateBody();
                
                if (!_cancelSlamFunc())
                {
                    abilityCast.ResetCastTime();
                    UnblockFSM();
                    _bodyCurrentRotationSpeed = _bodyNormalRotationSpeed;
                    yield break;
                }
                yield return null;
            }
            
            _bodyCurrentRotationSpeed = _bodyNormalRotationSpeed;
            
            AudioManager.instance.PlayOneShot(FMODEvents.instance.enemyAttack, transform.position);
            
            areaAbility.Activate();
            
            ContinueNavigation();
            
            StartCoroutine(StartCooldownCoroutine(areaAbility.GetCast()));
        }

        #endregion

        #endregion

        public override void OnReceiveDamage(uint damageValue, Vector3 hitPosition, Vector3 sourcePosition)
        {
            if (_itGoesOnAutomatic)
            {
                return;
            }
            base.OnReceiveDamage(damageValue, hitPosition, sourcePosition);
        }

        protected override void PreDeath()
        {
            base.PreDeath();
            _update = () => { };
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CombatManager.Instance.OnEnemyDefeated(this, _areaNumber);
        }
        
        /////////////////////////DEBUG
        
        [SerializeField] private bool _showDetectionAreaOfSlam;
        [SerializeField] private Color _colorOfDetectionAreaOfSlam;
    }
} 