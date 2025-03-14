using System;
using System.Collections;
using System.Collections.Generic;
using AI.Combat.AbilitySpecs;
using AI.Combat.Contexts;
using AI.Combat.Enemy.Sendatu;
using AI.Combat.ScriptableObjects;
using Interfaces.AI.Combat;
using Managers;
using UnityEngine;

namespace ECS.Entities.AI.Combat
{
    public class Sendatu : TeleportMobilityEnemy<SendatuContext, SendatuAction>
    {
        [SerializeField] private LongArmsProperties _sendatuProperties;
        
        
        
        private void Start()
        {
            CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
            float radius = capsuleCollider.radius;
            
            EnemySetup(radius, _sendatuProperties, EntityType.SENDATU, EntityType.SENDATU);

            _utilityFunction = new SendatuUtilityFunction();

            _bodyNormalRotationSpeed = _sendatuProperties.bodyNormalRotationSpeed;
            _bodyCurrentRotationSpeed = _bodyNormalRotationSpeed;

            _context = new SendatuContext(_sendatuProperties.totalHealth, radius, capsuleCollider.height, _headTransform, 
                _bodyTransform, _sendatuProperties.radiusToFlee);
            
            SetDirectionToRotateBody(transform.forward);
            
            CombatManager.Instance.AddEnemy(this);
        }

        protected override void InitiateDictionaries()
        {
            _actions = new Dictionary<SendatuAction, Action>
            {
                
            };
        }
        
        protected override void CreateAbilities()
        {
            //TODO SENDATU CREATE ABILITIES
        }

        #region AI LOOP

        private void Update()
        {
            UpdateVisibleTargets();
            
            UpdateVectorsToTargets();
            
            if (_context.IsFSMBlocked())
            {
                return;
            }
            
            RotateBody();
            
            CalculateBestAction();
        }

        protected override void UpdateVisibleTargets()
        {
            Vector3 position = transform.position;
            
            /*_visibleTargetsForThrowRock = CombatManager.Instance.ReturnVisibleTargets(
                    _sendatuProperties.throwRockAbilityProperties.abilityTarget, position,
                    _context.GetSightMaximumDistance());
            
            _context.SetIsSeeingATargetForThrowRock(_visibleTargetsForThrowRock.Count != 0);

            _visibleTargetsForClapAbove = CombatManager.Instance.ReturnVisibleTargets(
                    _sendatuProperties.clapAboveAbilityProperties.abilityTarget, position,
                    _context.GetSightMaximumDistance());
            
            _context.SetIsSeeingATargetForClampAbove(_visibleTargetsForClapAbove.Count != 0);*/
            
            
        }

        private void UpdateVectorsToTargets()
        {
            Vector3 targetPosition;
            Vector3 agentPosition;
            
            /*if (_context.HasATargetForThrowRock())
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
            
            _context.GetClapAboveTargetContext().SetVectorToTarget(targetPosition - agentPosition);*/
        }

        #endregion

        #region FSM

        #endregion

        public override SendatuContext GetContext()
        {
            return _context;
        }

        #region Abilities Managing

        #region Throw Rock

        private void StartCastingThrowRock(IProjectileAbility projectileAbility)
        {
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
                
                //if (_cancelThrowRockFunc())
                {
                    abilityCast.ResetCastTime();
                    _bodyCurrentRotationSpeed = _bodyNormalRotationSpeed;
                    UnblockFSM();
                    yield break;
                }
                yield return null;
            }

            _bodyCurrentRotationSpeed = _bodyNormalRotationSpeed;

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
                
                //if (_cancelClapAboveFunc())
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

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CombatManager.Instance.OnEnemyDefeated(this, _areaNumber);
        }

        protected override void RemoveATargetIfWasLost(uint targetIdToCheck)
        {
            throw new NotImplementedException();
        }

        public override void OnReceivePushFromCenter(Vector3 centerPosition, Vector3 forceDirection, float forceStrength, 
            Vector3 sourcePosition, ForceMode forceMode)
        {}

        public override void OnReceivePushInADirection(Vector3 colliderForwardVector, Vector3 forceDirection, float forceStrength, 
            Vector3 sourcePosition, ForceMode forceMode)
        {}
    }
}