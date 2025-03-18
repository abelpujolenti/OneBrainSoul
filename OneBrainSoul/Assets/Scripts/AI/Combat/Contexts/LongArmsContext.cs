using System.Collections.Generic;
using AI.Combat.AbilitySpecs;
using AI.Combat.Contexts.Target;
using AI.Combat.Enemy.LongArms;
using ECS.Entities;
using Interfaces.AI.UBS.Enemy.TeleportMobilityEnemy.LongArms;
using UnityEngine;

namespace AI.Combat.Contexts
{
    public class LongArmsContext : TeleportMobilityEnemyContext, ILongArmsIdleUtility, ILongArmsAcquireNewTargetForThrowRockUtility, 
        ILongArmsAcquireNewTargetForClapAboveUtility, ILongArmsThrowRockUtility, ILongArmsClapAboveUtility, ILongArmsFleeUtility
    {        
        private float _distanceToClosestTargetToFleeFrom;
        private uint _longArmsBasesFree;
        private float _radiusToFlee;

        private bool _isSeeingATargetForThrowRock;
        private bool _throwRockAbilityHasATarget;
        private AbilityCast _throwRockCast;
        private HashSet<uint> _targetsInsideThrowRockDetectionArea = new HashSet<uint>();
        private TargetContext _throwRockTarget = new TargetContext();

        private bool _isSeeingATargetForClapAbove;
        private bool _clapAboveAbilityHasATarget;
        private AbilityCast _clapAboveCast;
        private HashSet<uint> _targetsInsideClapAboveDetectionArea = new HashSet<uint>();
        private TargetContext _clapAboveTarget = new TargetContext();
        
        public LongArmsContext(uint totalHealth, float radius, float height, Transform headAgentTransform, 
            Transform bodyAgentTransform, AbilityCast throwRockCast, AbilityCast clapAboveCast, float radiusToFlee) 
            : base(EntityType.LONG_ARMS, totalHealth, radius, height, headAgentTransform, bodyAgentTransform)
        {
            _repeatableActions = new List<uint>
            {
                (uint)LongArmsAction.OBSERVE,
                (uint)LongArmsAction.GO_TO_CLOSEST_SIGHTED_TARGET,
                (uint)LongArmsAction.THROW_ROCK
            };

            _throwRockCast = throwRockCast;
            _clapAboveCast = clapAboveCast;
            _radiusToFlee = radiusToFlee;
        }

        public void SetIsSeeingATargetForThrowRock(bool isSeeingATarget)
        {
            _isSeeingATargetForThrowRock = isSeeingATarget;
        }

        public bool IsSeeingATargetForThrowRock()
        {
            return _isSeeingATargetForThrowRock;
        }

        public bool HasATargetForThrowRock()
        {
            return _throwRockAbilityHasATarget;
        }

        public bool IsThrowRockOnCooldown()
        {
            return _throwRockCast.IsOnCooldown();
        }

        public TargetContext GetThrowRockTargetContext()
        {
            return _throwRockTarget;
        }

        public void AddTargetInsideThrowRockDetectionArea(uint targetId)
        {
            _targetsInsideThrowRockDetectionArea.Add(targetId);
        }

        public void RemoveTargetInsideThrowRockDetectionArea(uint targetId)
        {
            _targetsInsideThrowRockDetectionArea.Remove(targetId);
        }

        public bool IsThrowRockTargetInsideDetectionArea()
        {
            return _targetsInsideThrowRockDetectionArea.Contains(_throwRockTarget.GetTargetId());
        }

        public void SetThrowRockTargetProperties(uint targetId, float targetRadius, float targetHeight)
        {
            SetIsFighting(true);
            _throwRockTarget.SetTargetProperties(targetId, targetRadius, targetHeight);

            _throwRockAbilityHasATarget = true;
        }

        public void LoseThrowRockTarget()
        {
            SetIsFighting(HasATarget());
            
            _throwRockAbilityHasATarget = false;
        }

        public void SetIsSeeingATargetForClapAbove(bool isSeeingATarget)
        {
            _isSeeingATargetForClapAbove = isSeeingATarget;
        }

        public bool IsSeeingATargetForClapAbove()
        {
            return _isSeeingATargetForClapAbove;
        }

        public bool HasATargetForClapAbove()
        {
            return _clapAboveAbilityHasATarget;
        }

        public bool IsClapAboveOnCooldown()
        {
            return _clapAboveCast.IsOnCooldown();
        }

        public TargetContext GetClapAboveTargetContext()
        {
            return _clapAboveTarget;
        }

        public void AddTargetInsideClapAboveDetectionArea(uint targetId)
        {
            _targetsInsideClapAboveDetectionArea.Add(targetId);
        }

        public void RemoveTargetInsideClapAboveDetectionArea(uint targetId)
        {
            _targetsInsideClapAboveDetectionArea.Remove(targetId);
        }

        public bool IsClapAboveTargetInsideDetectionArea()
        {
            return _targetsInsideClapAboveDetectionArea.Contains(_clapAboveTarget.GetTargetId());
        }

        public void SetClapAboveTargetProperties(uint targetId, float targetRadius, float targetHeight)
        {
            SetIsFighting(true);
            _clapAboveTarget.SetTargetProperties(targetId, targetRadius, targetHeight);

            _clapAboveAbilityHasATarget = true;
        }

        public void LoseClapAboveTarget()
        {
            SetIsFighting(HasATarget());
            
            _clapAboveAbilityHasATarget = false;
        }

        public void IncrementLongArmsBasesFree()
        {
            _longArmsBasesFree++;
        }

        public void DecrementLongArmsBasesFree()
        {
            _longArmsBasesFree--;
        }

        public uint GetLongArmsBasesFree()
        {
            return _longArmsBasesFree;
        }

        public float GetDistanceToClosestTargetToFleeFrom()
        {
            return _distanceToClosestTargetToFleeFrom;
        }

        public void SetDistanceToClosestTargetToFleeFrom(float distance)
        {
            _distanceToClosestTargetToFleeFrom = distance;
        }

        public float GetRadiusToFlee()
        {
            return _radiusToFlee;
        }

        public override bool IsSeeingATarget()
        {
            return IsSeeingATargetForThrowRock() || IsSeeingATargetForClapAbove();
        }

        public override bool HasATarget()
        {
            return HasATargetForThrowRock() || HasATargetForClapAbove();
        }
    }
}