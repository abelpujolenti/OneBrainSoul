using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        private float _bodyRotationSpeedWhenAcquiringATarget;
        private float _bodyRotationSpeedWhileCastingThrowRock;
        private float _bodyRotationSpeedWhenTurningAround;

        private float _minimumTimeBeforeSettingNewDirection;
        private float _maximumTimeBeforeSettingNewDirection;

        private uint _bodyMinimumDegreesToRotate;
        private uint _bodyMaximumDegreesToRotate;

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

            EnemySetup(radius, _longArmsProperties, EntityType.LONG_ARMS,
                _longArmsProperties.throwRockAbilityProperties.abilityTarget |
                _longArmsProperties.clapAboveAbilityProperties.abilityTarget);

            _utilityFunction = new LongArmsUtilityFunction();

            _bodyNormalRotationSpeed = _longArmsProperties.bodyNormalRotationSpeed;
            _bodyRotationSpeedWhenAcquiringATarget = _longArmsProperties.bodyRotationSpeedWhenAcquiringATarget;
            _bodyRotationSpeedWhileCastingThrowRock = _longArmsProperties.bodyNotationSpeedWhileCastingThrowRock;
            _bodyRotationSpeedWhenTurningAround = _longArmsProperties.bodyRotationSpeedWhileTurningAround;
            _bodyCurrentRotationSpeed = _bodyNormalRotationSpeed;

            _minimumTimeBeforeSettingNewDirection = _longArmsProperties.minimumTimeBeforeSettingNewDirection;
            _maximumTimeBeforeSettingNewDirection = _longArmsProperties.maximumTimeBeforeSettingNewDirection;

            _bodyMinimumDegreesToRotate = _longArmsProperties.bodyMinimumDegreesToRotateDirection;
            _bodyMaximumDegreesToRotate = _longArmsProperties.bodyMaximumDegreesToRotateDirection;

            _minimumTimesSettingNewDirectionToTurnAround = _longArmsProperties.minimumTimesSettingNewDirectionToTurnAround;
            _maximumTimesSettingNewDirectionToTurnAround = _longArmsProperties.maximumTimesSettingNewDirectionToTurnAround;

            _context = new LongArmsContext(_longArmsProperties.totalHealth, _longArmsProperties.maximumHeadYawRotation, 
                radius, capsuleCollider.height, _longArmsProperties.sightMaximumDistance, _longArmsProperties.fov, _headTransform, 
                _bodyTransform, _throwRockAbility.GetCast(), _clapAboveAbility.GetCast(), _longArmsProperties.radiusToFlee);
            
            SetDirectionToRotateBody(transform.forward);
            
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
                { LongArmsAction.OBSERVE , Observing },
                { LongArmsAction.GO_TO_CLOSEST_SIGHTED_TARGET , GoToClosestTargetSighted },
                { LongArmsAction.ACQUIRE_NEW_TARGET_FOR_THROW_ROCK , AcquireNewTargetForThrowRock },
                { LongArmsAction.ACQUIRE_NEW_TARGET_FOR_CLAP_ABOVE , AcquireNewTargetForClapAbove },
                { LongArmsAction.THROW_ROCK , ThrowRock },
                { LongArmsAction.CLAP_ABOVE , ClapAbove },
                { LongArmsAction.FLEE , Flee }
            };
        }
        
        protected override void CreateAbilities()
        {
            base.CreateAbilities();
            
            _throwRockAbility = AbilityManager.Instance.ReturnProjectileAbility(_longArmsProperties.throwRockAbilityProperties,
                transform);

            if (_throwRockAbility.GetCast().canCancelCast)
            {
                _cancelThrowRockFunc = () =>
                {
                    AbilityCast abilityCast = _throwRockAbility.GetCast();
                    Vector3 vectorToTarget = _context.GetThrowRockTargetContext().GetVectorToTarget();

                    return Vector3.Angle(_context.GetDirectionOfThrowRockDetection(), vectorToTarget) > abilityCast.maximumAngleToCancelCast ||
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

                return Vector3.Angle(_context.GetDirectionOfClapAboveDetection(), vectorToTarget) > abilityCast.maximumAngleToCancelCast ||
                    vectorToTarget.sqrMagnitude > abilityCast.maximumRangeToCast * abilityCast.maximumRangeToCast;
            };
        }

        #region AI LOOP

        private void Update()
        {
            UpdateSightedTargetsInsideCombatArea();
            
            UpdateVisibleTargets();
            
            UpdateVectorsToTargets();
            
            if (_context.IsFSMBlocked())
            {
                return;
            }
            
            //RotateHead();
            
            RotateBody();
            
            CalculateBestAction();
        }

        protected override void UpdateSightedTargetsInsideCombatArea()
        {
            base.UpdateSightedTargetsInsideCombatArea();

            if (_context.HasATargetForThrowRock() && !_targetsSightedInsideCombatArea.Contains(_throwRockAbility.GetTargetId()))
            {
                _context.LoseThrowRockTarget();
            }

            if (!_context.HasATargetForClapAbove() || _targetsSightedInsideCombatArea.Contains(_clapAboveAbility.GetTargetId()))
            {
                return;
            }
            
            _context.LoseClapAboveTarget();
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
                    _longArmsProperties.throwRockAbilityProperties.abilityTarget, position, _targetsInsideVisionArea, _areaNumber);
            
            _context.SetIsSeeingATargetForThrowRock(_visibleTargetsForThrowRock.Count != 0);

            _visibleTargetsForClapAbove = CombatManager.Instance.ReturnVisibleTargets(
                    _longArmsProperties.clapAboveAbilityProperties.abilityTarget, position, _targetsInsideVisionArea, _areaNumber);
            
            _context.SetIsSeeingATargetForClapAbove(_visibleTargetsForClapAbove.Count != 0);

            _visibleTargetsToFleeFrom = CombatManager.Instance.ReturnVisibleTargets(_longArmsProperties.entitiesToFleeFrom, 
                position, _targetsInsideVisionArea, _areaNumber);
        }

        private void UpdateDistancesToTargetsToFleeFrom()
        {
            Vector3 agentPosition = transform.position;
            
            foreach (uint targetId in _visibleTargetsToFleeFrom)
            {
                _context.SetDistanceToTargetToFleeFrom(targetId,
                    CombatManager.Instance.ReturnDistanceToTarget(agentPosition, targetId));
            }
        }

        private void UpdateVectorsToTargets()
        {
            Vector3 agentPosition = transform.position;

            Vector3 targetPosition;
            Vector3 targetVelocity;
            Vector3 vectorToTarget;
            
            UpdateDistancesToTargetsToFleeFrom();
            
            if (_context.HasATargetForThrowRock())
            {
                {
                    AgentEntity target = CombatManager.Instance.ReturnAgentEntity(_throwRockAbility.GetTargetId());
                    targetPosition = target.GetTransformComponent().GetPosition();
                    targetVelocity = target.GetVelocity();
                }
            
                _context.GetThrowRockTargetContext().SetTargetState(targetPosition, targetVelocity);
                    
                agentPosition = transform.position;
            
                targetPosition.y -= _context.GetThrowRockTargetContext().GetTargetHeight() / 2;
                agentPosition.y -= _context.GetHeight() / 2;

                vectorToTarget = targetPosition - agentPosition;
            
                _context.GetThrowRockTargetContext().SetVectorToTarget(vectorToTarget);
                    
                SetDirectionToRotateBody(vectorToTarget);
            }

            if (!_context.HasATargetForClapAbove())
            {
                return;
            }

            {
                AgentEntity target = CombatManager.Instance.ReturnAgentEntity(_clapAboveAbility.GetTargetId());
                targetPosition = target.GetTransformComponent().GetPosition();
                targetVelocity = target.GetVelocity();
            }
            
            _context.GetClapAboveTargetContext().SetTargetState(targetPosition, targetVelocity);
            
            agentPosition = transform.position;
            
            targetPosition.y -= _context.GetClapAboveTargetContext().GetTargetHeight() / 2;
            agentPosition.y -= _context.GetHeight() / 2;

            vectorToTarget = targetPosition - agentPosition;
            
            _context.GetClapAboveTargetContext().SetVectorToTarget(vectorToTarget);

            if (_context.HasATargetForThrowRock())
            {
                return;
            }
            
            SetDirectionToRotateBody(vectorToTarget);
        }

        #endregion

        #region FSM

        private void Observing()
        {
            ShowDebugMessages("Long Arms " + GetAgentID() + " Observing");

            if (_targetsSightedInsideCombatArea.Any())
            {
                Vector3 position = transform.position;

                SetDirectionToRotateBody(CombatManager.Instance.ReturnClosestAgentEntity(position, _targetsSightedInsideCombatArea)
                    .GetTransformComponent().GetPosition() - position);
                return;
            }

            if (_isSettingNewDirectionToRotate || Vector3.Dot(transform.forward, GetDirectionToRotateBody()) < 0.95f)
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

            if (_context.IsSeeingATarget())
            {
                yield break;
            }

            if (_timesSettingNewDirection != 0)
            {
                float degrees = Random.Range(_bodyMinimumDegreesToRotate, _bodyMaximumDegreesToRotate) * (Random.Range(0, 2) * 2 - 1);

                SetDirectionToRotateBody(MathUtil.RotateVector(GetDirectionToRotateBody(), Vector3.up, degrees));

                _timesSettingNewDirection--;
                yield break;
            }
            
            BlockFSM();
            
            TurnAround();
        }

        private void TurnAround()
        {
            ShowDebugMessages("Long Arms " + GetAgentID() + " Turning Around");

            SetDirectionToRotateBody(transform.forward * -1);

            _bodyCurrentRotationSpeed = _bodyRotationSpeedWhenTurningAround;

            StartCoroutine(BackToNormalRotationSpeed());
        }

        private IEnumerator BackToNormalRotationSpeed()
        {
            while (!_context.IsSeeingATarget() && transform.forward != GetDirectionToRotateBody())
            {
                RotateBody();
                yield return null;
            }

            _timesSettingNewDirection = (uint)Random.Range(_minimumTimesSettingNewDirectionToTurnAround,
                _maximumTimesSettingNewDirectionToTurnAround);

            _bodyCurrentRotationSpeed = _bodyNormalRotationSpeed;
            
            UnblockFSM();
        }

        private void GoToClosestTargetSighted()
        {
            
        }

        private void AcquireNewTargetForThrowRock()
        {
            ShowDebugMessages("Long Arms " + GetAgentID() + " Acquiring New Target For Throw Rock");
            
            Vector3 position = transform.position;
            position.y -= GetHeight() / 2;
            
            AgentEntity target = CombatManager.Instance.ReturnClosestAgentEntity(position, _visibleTargetsForThrowRock);

            _bodyCurrentRotationSpeed = _bodyRotationSpeedWhenAcquiringATarget;
            
            _context.SetThrowRockTargetProperties(target.GetRadius(), target.GetHeight());
            
            _throwRockAbility.SetTargetId(target.GetAgentID());
        }

        private void AcquireNewTargetForClapAbove()
        {
            ShowDebugMessages("Long Arms " + GetAgentID() + " Acquiring New Target For Clap Above");
            
            Vector3 position = transform.position;
            position.y -= GetHeight() / 2;
            
            AgentEntity target = CombatManager.Instance.ReturnClosestAgentEntity(position, _visibleTargetsForClapAbove);

            _bodyCurrentRotationSpeed = _bodyRotationSpeedWhenAcquiringATarget;
            
            _context.SetClapAboveTargetProperties(target.GetRadius(), target.GetHeight());
            
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
            
            BlockFSM();
            
            //_animator.
            
            TeleportToAnotherLongArmsBase();
        }

        private void TeleportToAnotherLongArmsBase()
        {
            CombatManager.Instance.RequestFleeToAnotherLongArmsBase(this);
            
            UpdateDistancesToTargetsToFleeFrom();
            
            //_animator.
            
            UnblockFSM();
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
            _bodyCurrentRotationSpeed = _bodyRotationSpeedWhileCastingThrowRock;
            
            BlockFSM();
            
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
                
                RotateBody();
                
                if (_cancelThrowRockFunc())
                {
                    abilityCast.ResetCastTime();
                    UnblockFSM();
                    yield break;
                }
                yield return null;
            }

            if (!projectileAbility.FIREEEEEEEEEEEEEE())
            {
                UnblockFSM();
                yield break;
            }
            StartCoroutine(StartCooldownCoroutine(projectileAbility.GetCast()));
        }

        #endregion

        #region Clap Above

        private void StartCastingClapAbove(IAreaAbility areaAbility) 
        {
            BlockFSM();
            
            StartCoroutine(StartClapAboveCastTimeCoroutine(areaAbility));
        }

        private IEnumerator StartClapAboveCastTimeCoroutine(IAreaAbility areaAbility)
        {
            AbilityCast abilityCast = areaAbility.GetCast();
            
            abilityCast.StartCastTime();

            while (abilityCast.IsCasting())
            {
                abilityCast.DecreaseCurrentCastTime();
                
                RotateBody();
                
                if (_cancelClapAboveFunc())
                {
                    abilityCast.ResetCastTime();
                    UnblockFSM();
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

        public void SetLongArmsBaseIdFunc(Func<uint> longArmsBaseIdFunc)
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
            CombatManager.Instance.OnEnemyDefeated(this, _areaNumber);
        }

        public override void OnReceivePushFromCenter(Vector3 centerPosition, Vector3 forceDirection, float forceStrength, Vector3 sourcePosition)
        {}

        public override void OnReceivePushInADirection(Vector3 colliderForwardVector, Vector3 forceDirection, float forceStrength, Vector3 sourcePosition)
        {}
        
        /////////////////////////DEBUG

        [SerializeField] private bool _showDetectionAreaOfThrowRock;
        [SerializeField] private Color _colorOfDetectionAreaOfThrowRock;
        [SerializeField] private bool _showDetectionAreaOfClapAbove;
        [SerializeField] private Color _colorOfDetectionAreaOfClapAbove;
        [SerializeField] private bool _showDetectionAreaOfFlee;
        [SerializeField] private Color _colorOfDetectionAreaOfFlee;
        
        private void OnDrawGizmos()
        {
            Vector3 origin = _headTransform.position;
            int segments = 20;
            
            base.OnDrawGizmos();
            
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

            if (_showDetectionAreaOfFlee)
            {
                Gizmos.color = _colorOfDetectionAreaOfFlee;
                Gizmos.DrawSphere(_bodyTransform.position, _context.GetRadiusToFlee());
            }
        }
    }
}