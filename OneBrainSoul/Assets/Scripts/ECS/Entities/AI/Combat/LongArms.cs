using System;
using System.Collections;
using System.Collections.Generic;
using AI.Combat.AbilitySpecs;
using AI.Combat.Contexts;
using AI.Combat.Enemy.LongArms;
using AI.Combat.ScriptableObjects;
using Interfaces.AI.Combat;
using Managers;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace ECS.Entities.AI.Combat
{
    public class LongArms : TeleportMobilityEnemy<LongArmsContext, LongArmsAction>
    {
        [SerializeField] private LongArmsProperties _longArmsProperties;

        private IProjectileAbility _throwRockAbility;
        private HashSet<uint> _visibleTargetsForThrowRock;
        private Func<bool> _cancelThrowRockFunc = () => false;

        private IAreaAbility _clapAboveAbility;
        private HashSet<uint> _visibleTargetsForClapAbove;
        private Func<bool> _cancelClapAboveFunc = () => false;

        private HashSet<uint> _visibleTargetsToFleeFrom;

        private float _rotationSpeedWhenAcquiringATarget;
        private float _rotationSpeedWhileCastingThrowRock;
        private float _rotationSpeedWhenTurningAround;

        private float _minimumTimeBeforeSettingNewDirection;
        private float _maximumTimeBeforeSettingNewDirection;

        private uint _minimumDegreesToRotate;
        private uint _maximumDegreesToRotate;

        private uint _minimumTimesSettingNewDirectionToTurnAround;
        private uint _maximumTimesSettingNewDirectionToTurnAround;
        private uint _timesSettingNewDirection;

        private bool _isSettingNewDirectionToRotate;

        private Action _onFlee;
        private Func<uint> _longArmsBaseIdFunc;
        
        private void Start()
        {
            CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
            float radius = capsuleCollider.radius;
            
            EnemySetup(radius, _longArmsProperties, EntityType.LONG_ARMS);

            _utilityFunction = new LongArmsUtilityFunction();

            _normalRotationSpeed = _longArmsProperties.normalRotationSpeed;
            _rotationSpeedWhenAcquiringATarget = _longArmsProperties.rotationSpeedWhenAcquiringATarget;
            _rotationSpeedWhileCastingThrowRock = _longArmsProperties.rotationSpeedWhileCastingThrowRock;
            _rotationSpeedWhenTurningAround = _longArmsProperties.rotationSpeedWhileTurningAround;
            _currentRotationSpeed = _normalRotationSpeed;

            _minimumTimeBeforeSettingNewDirection = _longArmsProperties.minimumTimeBeforeSettingNewDirection;
            _maximumTimeBeforeSettingNewDirection = _longArmsProperties.maximumTimeBeforeSettingNewDirection;

            _minimumDegreesToRotate = _longArmsProperties.minimumDegreesToRotateDirection;
            _maximumDegreesToRotate = _longArmsProperties.maximumDegreesToRotateDirection;

            _minimumTimesSettingNewDirectionToTurnAround = _longArmsProperties.minimumTimesSettingNewDirectionToTurnAround;
            _maximumTimesSettingNewDirectionToTurnAround = _longArmsProperties.maximumTimesSettingNewDirectionToTurnAround;

            _context = new LongArmsContext(_longArmsProperties.totalHealth, radius, capsuleCollider.height,
                _longArmsProperties.sightMaximumDistance, _longArmsProperties.fov, transform, _throwRockAbility.GetCast(), 
                _clapAboveAbility.GetCast(), _longArmsProperties.radiusToFlee);
            
            SetDirectionToRotate(transform.forward);
            
            LongArmsBase longArmsBase = transform.parent.GetComponent<LongArmsBase>(); 
            
            longArmsBase.SetLongArms(this);
            
            CombatManager.Instance.AddEnemy(this);
            
            _timesSettingNewDirection = (uint)Random.Range(_minimumTimesSettingNewDirectionToTurnAround,
                _maximumTimesSettingNewDirectionToTurnAround);
        }

        protected override void InitiateDictionaries()
        {
            _actions = new Dictionary<LongArmsAction, Action>
            {
                { LongArmsAction.OBSERVING , Observing },
                { LongArmsAction.TURN_AROUND , TurnAround },
                { LongArmsAction.ACQUIRE_NEW_TARGET_FOR_THROW_ROCK , AcquireNewTargetForThrowRock },
                { LongArmsAction.ACQUIRE_NEW_TARGET_FOR_CLAP_ABOVE , AcquireNewTargetForClapAbove },
                { LongArmsAction.THROW_ROCK , ThrowRock },
                { LongArmsAction.CLAP_ABOVE , ClapAbove },
                { LongArmsAction.FLEE , Flee }
            };
        }
        
        protected override void CreateAbilities()
        {
            _throwRockAbility = AbilityManager.Instance.ReturnProjectileAbility(_longArmsProperties.throwRockAbilityProperties,
                transform);

            if (_throwRockAbility.GetCast().canCancelCast)
            {
                _cancelThrowRockFunc = () =>
                {
                    AbilityCast abilityCast = _throwRockAbility.GetCast();
                    Vector3 vectorToTarget = _context.GetThrowRockTargetContext().GetVectorToTarget();

                    return Vector3.Angle(transform.forward, vectorToTarget) > abilityCast.maximumAngleToCancelCast ||
                           vectorToTarget.sqrMagnitude > abilityCast.maximumRangeToCast * abilityCast.maximumRangeToCast;
                };
            }
            
            _clapAboveAbility = AbilityManager.Instance.ReturnAreaAbility(_longArmsProperties.clapAboveAbilityProperties,
                transform);

            if (!_clapAboveAbility.GetCast().canCancelCast)
            {
                return;
            }

            _cancelClapAboveFunc = () =>
            {
                AbilityCast abilityCast = _clapAboveAbility.GetCast();
                Vector3 vectorToTarget = _context.GetClapAboveTargetContext().GetVectorToTarget();

                return Vector3.Angle(transform.forward, vectorToTarget) > abilityCast.maximumAngleToCancelCast ||
                    vectorToTarget.sqrMagnitude > abilityCast.maximumRangeToCast * abilityCast.maximumRangeToCast;
            };
        }

        #region AI LOOP

        private void Update()
        {
            UpdateVisibleTargets();
            
            UpdateVectorsToTargets();
            
            if (_context.IsCastingAnAbility())
            {
                return;
            }
            
            Rotate();
            
            CalculateBestAction();
        }

        protected override void UpdateVisibleTargets()
        {
            Transform ownTransform = transform;
            
            Vector3 position = ownTransform.position;
            position.y += GetHeight() / 2;

            Vector3 forward = ownTransform.forward;

            float sightMaximumDistance = _context.GetSightMaximumDistance();

            float fov = _context.GetFov();
            
            _visibleTargetsForThrowRock = CombatManager.Instance.ReturnVisibleTargets(
                    _longArmsProperties.throwRockAbilityProperties.abilityTarget, position, sightMaximumDistance, 
                    forward, fov);
            
            _context.SetIsSeeingATargetForThrowRock(_visibleTargetsForThrowRock.Count != 0);

            _visibleTargetsForClapAbove = CombatManager.Instance.ReturnVisibleTargets(
                    _longArmsProperties.clapAboveAbilityProperties.abilityTarget, position, sightMaximumDistance, 
                    forward, fov);
            
            _context.SetIsSeeingATargetForClapAbove(_visibleTargetsForClapAbove.Count != 0);

            _visibleTargetsToFleeFrom = CombatManager.Instance.ReturnVisibleTargets(_longArmsProperties.entitiesToFleeFrom, 
                position, sightMaximumDistance, forward, fov);
        }

        private void UpdateVectorsToTargets()
        {
            Vector3 targetPosition;
            Vector3 agentPosition = transform.position;

            foreach (uint targetId in _visibleTargetsToFleeFrom)
            {
                _context.SetDistanceToTargetToFleeFrom(targetId,
                    CombatManager.Instance.ReturnDistanceToTarget(agentPosition, targetId));
            }

            if (!_context.HasATarget())
            {
                //_currentRotationSpeed = _normalRotationSpeed;
            }
            
            if (_context.HasATargetForThrowRock())
            {
                targetPosition = _context.GetThrowRockTargetContext().GetTargetTransform().position;
                agentPosition = transform.position;
            
                targetPosition.y -= _context.GetThrowRockTargetContext().GetTargetHeight() / 2;
                agentPosition.y -= _context.GetHeight() / 2;

                Vector3 vectorToTarget = targetPosition - agentPosition;
                
                SetDirectionToRotate(vectorToTarget);
            
                _context.GetThrowRockTargetContext().SetVectorToTarget(vectorToTarget);
            }

            if (!_context.HasATargetForClapAbove())
            {
                return;
            }
            
            targetPosition = _context.GetClapAboveTargetContext().GetTargetTransform().position;
            agentPosition = transform.position;
            
            targetPosition.y -= _context.GetClapAboveTargetContext().GetTargetHeight() / 2;
            agentPosition.y -= _context.GetHeight() / 2;
            
            _context.GetClapAboveTargetContext().SetVectorToTarget(targetPosition - agentPosition);
        }

        #endregion

        #region FSM

        private void Observing()
        {
            ShowDebugMessages("Long Arms " + GetAgentID() + " Observing");

            if (_isSettingNewDirectionToRotate || transform.forward != GetDirectionToRotate())
            {
                return;
            }

            _isSettingNewDirectionToRotate = true;
            StartCoroutine(SetNewDirectionToRotate(Random.Range(_minimumTimeBeforeSettingNewDirection, _maximumTimeBeforeSettingNewDirection)));
        }

        private IEnumerator SetNewDirectionToRotate(float delay)
        {
            float timer = 0;

            while (timer < delay)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            
            _isSettingNewDirectionToRotate = false;

            if (_timesSettingNewDirection != 0)
            {
                float degrees = Random.Range(_minimumDegreesToRotate, _maximumDegreesToRotate) * (Random.Range(0, 2) * 2 - 1);

                SetDirectionToRotate(MathUtil.RotateVector(GetDirectionToRotate(), Vector3.up, degrees));

                _timesSettingNewDirection--;
                yield break;
            }
            
            TurnAround();
        }

        private void TurnAround()
        {
            ShowDebugMessages("Long Arms " + GetAgentID() + " Turning Around");

            SetDirectionToRotate(transform.forward * -1);

            _currentRotationSpeed = _rotationSpeedWhenTurningAround;

            StartCoroutine(BackToNormalRotationSpeed());
        }

        private IEnumerator BackToNormalRotationSpeed()
        {
            while (!_context.IsSeeingATarget() && transform.forward != GetDirectionToRotate())
            {
                yield return null;
            }

            _timesSettingNewDirection = (uint)Random.Range(_minimumTimesSettingNewDirectionToTurnAround,
                _maximumTimesSettingNewDirectionToTurnAround);

            _currentRotationSpeed = _normalRotationSpeed;
        }

        private void AcquireNewTargetForThrowRock()
        {
            ShowDebugMessages("Long Arms " + GetAgentID() + " Acquiring New Target For Throw Rock");
            
            Vector3 position = transform.position;
            position.y -= GetHeight() / 2;
            
            AgentEntity target = CombatManager.Instance.ReturnClosestAgentEntity(position, _visibleTargetsForThrowRock);

            _currentRotationSpeed = _rotationSpeedWhenAcquiringATarget;
            
            _context.SetThrowRockTarget(target);
            
            _throwRockAbility.SetTargetId(target.GetAgentID());
        }

        private void AcquireNewTargetForClapAbove()
        {
            ShowDebugMessages("Long Arms " + GetAgentID() + " Acquiring New Target For Clap Above");
            
            Vector3 position = transform.position;
            position.y -= GetHeight() / 2;
            
            AgentEntity target = CombatManager.Instance.ReturnClosestAgentEntity(position, _visibleTargetsForClapAbove);

            _currentRotationSpeed = _rotationSpeedWhenAcquiringATarget;
            
            _context.SetClapAboveTarget(target);
            
            _clapAboveAbility.SetTargetId(target.GetAgentID());
        }

        private void ThrowRock()
        {
            ShowDebugMessages("Long Arms " + GetAgentID() + " Throwing Rock");
            
            StartCastingThrowRock(_throwRockAbility);
        }

        private void ClapAbove()
        {
            ShowDebugMessages("Long Arms " + GetAgentID() + " Clapping Above");
            
            StartCastingClapAbove(_clapAboveAbility);
        }

        private void Flee()
        {
            ShowDebugMessages("Long Arms " + GetAgentID() + " Fleeing");
            
            CastingAnAbility();
            
            //_animator.
            
            TeleportToAnotherLongArmsBase();
        }

        private void TeleportToAnotherLongArmsBase()
        {
            CombatManager.Instance.RequestFleeToAnotherLongArmsBase(this);
            
            //_animator.
            
            NotCastingAnAbility();
        }

        #endregion

        public void IncrementLongArmsFreeBases()
        {
            _context.IncrementLongArmsBasesFree();
        }

        public void DecrementLongArmsFreeBases()
        {
            _context.DecrementLongArmsBasesFree();
        }

        public override LongArmsContext GetContext()
        {
            return _context;
        }

        #region Abilities Managing

        #region Throw Rock

        private void StartCastingThrowRock(IProjectileAbility projectileAbility)
        {
            _currentRotationSpeed = _rotationSpeedWhileCastingThrowRock;
            
            CastingAnAbility();
            
            projectileAbility.Activate();
            
            StartCoroutine(StartThrowRockCastTimeCoroutine(projectileAbility));
        }

        private IEnumerator StartThrowRockCastTimeCoroutine(IProjectileAbility projectileAbility)
        {
            AbilityCast abilityCast = projectileAbility.GetCast();
            
            abilityCast.StartCastTime();

            while (abilityCast.IsCasting())
            {
                abilityCast.DecreaseCurrentCastTime();
                
                Rotate();
                
                if (_cancelThrowRockFunc())
                {
                    abilityCast.ResetCastTime();
                    NotCastingAnAbility();
                    yield break;
                }
                yield return null;
            }

            if (!projectileAbility.FIREEEEEEEEEEEEEE())
            {
                NotCastingAnAbility();
                yield break;
            }
            StartCoroutine(StartCooldownCoroutine(projectileAbility.GetCast()));
        }

        #endregion

        #region Clap Above

        private void StartCastingClapAbove(IAreaAbility areaAbility) 
        {
            CastingAnAbility();
            
            StartCoroutine(StartClapAboveCastTimeCoroutine(areaAbility));
        }

        private IEnumerator StartClapAboveCastTimeCoroutine(IAreaAbility areaAbility)
        {
            AbilityCast abilityCast = areaAbility.GetCast();
            
            abilityCast.StartCastTime();

            while (abilityCast.IsCasting())
            {
                abilityCast.DecreaseCurrentCastTime();
                
                Rotate();
                
                if (_cancelClapAboveFunc())
                {
                    abilityCast.ResetCastTime();
                    NotCastingAnAbility();
                    yield break;
                }
                yield return null;
            }
            
            AudioManager.instance.PlayOneShot(FMODEvents.instance.enemyAttack, transform.position);
            
            areaAbility.Activate();
            
            StartCoroutine(StartCooldownCoroutine(areaAbility.GetCast()));
        }

        #endregion

        #endregion

        public void SetOnFleeAction(Action onFlee)
        {
            _onFlee = onFlee;
        }

        public void CallOnFleeAction()
        {
            _onFlee();
        }

        public void SetOnDieAction(Func<uint> longArmsBaseIdFunc)
        {
            _longArmsBaseIdFunc = longArmsBaseIdFunc;
        }

        public uint CallLongArmsBaseIdFunc()
        {
            return _longArmsBaseIdFunc();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _onFlee();
            CombatManager.Instance.OnEnemyDefeated(this);
        }

        public override void OnReceivePushFromCenter(Vector3 centerPosition, Vector3 forceDirection, float forceStrength)
        {}

        public override void OnReceivePushInADirection(Vector3 colliderForwardVector, Vector3 forceDirection, float forceStrength)
        {}
        
        
        
        /////////////////////////DEBUG

        [SerializeField] private bool _showDetectionAreaOfThrowRock;
        [SerializeField] private Color _colorOfDetectionAreaOfThrowRock;
        [SerializeField] private bool _showDetectionAreaOfClapAbove;
        [SerializeField] private Color _colorOfDetectionAreaOfClapAbove;
        
        private void OnDrawGizmos()
        {
            Vector3 origin = transform.position;
            int segments = 20;

            if (_showFov)
            {
                DrawCone(_fovColor, _context.GetFov(), 0, _context.GetSightMaximumDistance(), origin, transform.forward, segments);
            }
            
            if (_showDetectionAreaOfThrowRock)
            {
                DrawAbilityCone(_colorOfDetectionAreaOfThrowRock, _context.HasATargetForThrowRock(), _throwRockAbility.GetCast(), origin,
                    _context.GetDirectionOfThrowRockDetection(), segments);
            }
            
            if (_showDetectionAreaOfClapAbove)
            {
                DrawAbilityCone(_colorOfDetectionAreaOfClapAbove, _context.HasATargetForClapAbove(), _clapAboveAbility.GetCast(), origin,
                    _context.GetDirectionOfClapAboveDetection(), segments);
            }
        }
    }
}