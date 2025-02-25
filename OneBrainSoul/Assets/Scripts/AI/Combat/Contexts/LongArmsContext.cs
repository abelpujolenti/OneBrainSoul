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
        private Dictionary<uint, float> _distancesToTargetsToFleeFrom;
        private uint _longArmsBasesFree;
        private float _radiusToFlee;

        private bool _isSeeingATargetForThrowRock;
        private bool _throwRockAbilityHasATarget;
        private AbilityCast _throwRockCast;
        private TargetContext _throwRockTarget = new TargetContext();

        private bool _isSeeingATargetForClapAbove;
        private bool _clapAboveAbilityHasATarget;
        private AbilityCast _clapAboveCast;
        private TargetContext _clapAboveTarget = new TargetContext();
        
        public LongArmsContext(uint totalHealth, uint maximumHeadYawRotation,float radius, float height, 
            float sightMaximumDistance, uint fov, Transform headAgentTransform, Transform bodyAgentTransform, 
            AbilityCast throwRockCast, AbilityCast clapAboveCast, float radiusToFlee) : base(EntityType.LONG_ARMS, 
            totalHealth, maximumHeadYawRotation,radius, height, sightMaximumDistance, fov, headAgentTransform, bodyAgentTransform)
        {
            _repeatableActions = new List<uint>
            {
                (uint)LongArmsAction.OBSERVE,
                (uint)LongArmsAction.GO_TO_CLOSEST_SIGHTED_TARGET,
                (uint)LongArmsAction.THROW_ROCK
            };

            _distancesToTargetsToFleeFrom = new Dictionary<uint, float>();
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

        public float GetThrowRockMinimRangeToCast()
        {
            return _throwRockCast.minimumRangeToCast;
        }

        public float GetThrowRockMaximRangeToCast()
        {
            return _throwRockCast.maximumRangeToCast;
        }

        public bool IsThrowRockOnCooldown()
        {
            return _throwRockCast.IsOnCooldown();
        }

        public TargetContext GetThrowRockTargetContext()
        {
            return _throwRockTarget;
        }

        public Vector3 GetDirectionOfThrowRockDetection()
        {
            Vector3 direction = _throwRockCast.directionOfDetection;

            direction = GetAgentHeadTransform().rotation * direction;
            
            return direction;
        }

        public float GetMinimumAngleFromForwardToCastThrowRock()
        {
            return _throwRockCast.minimumAngleToCast;
        }

        public void SetThrowRockTargetProperties(float targetRadius, float targetHeight)
        {
            SetIsFighting(true);
            _throwRockTarget.SetTargetProperties(targetRadius, targetHeight);

            _throwRockAbilityHasATarget = true;
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

        public float GetClapAboveMinimRangeToCast()
        {
            return _clapAboveCast.minimumRangeToCast;
        }

        public float GetClapAboveMaximRangeToCast()
        {
            return _clapAboveCast.maximumRangeToCast;
        }

        public bool IsClapAboveOnCooldown()
        {
            return _clapAboveCast.IsOnCooldown();
        }

        public TargetContext GetClapAboveTargetContext()
        {
            return _clapAboveTarget;
        }

        public Vector3 GetDirectionOfClapAboveDetection()
        {
            Vector3 direction = _clapAboveCast.directionOfDetection;

            direction = GetAgentHeadTransform().rotation * direction;
            
            return direction;
        }

        public float GetMinimumAngleFromForwardToCastClapAbove()
        {
            return _clapAboveCast.minimumAngleToCast;
        }

        public void SetClapAboveTargetProperties(float targetRadius, float targetHeight)
        {
            SetIsFighting(true);
            _clapAboveTarget.SetTargetProperties(targetRadius, targetHeight);

            _clapAboveAbilityHasATarget = true;
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
            float minimumDistance = Mathf.Infinity;

            foreach (float distance in _distancesToTargetsToFleeFrom.Values)
            {
                if (minimumDistance < distance)
                {
                    continue;
                }

                minimumDistance = distance;
            }
            
            return minimumDistance;
        }

        public void SetDistanceToTargetToFleeFrom(uint agentID, float distance)
        {
            if (_distancesToTargetsToFleeFrom.ContainsKey(agentID))
            {
                _distancesToTargetsToFleeFrom[agentID] = distance;
                return;
            }
            
            _distancesToTargetsToFleeFrom.Add(agentID, distance);
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