using System.Collections.Generic;
using AI.Combat.AbilitySpecs;
using AI.Combat.Contexts.Target;
using AI.Combat.Enemy.LongArms;
using ECS.Entities;
using ECS.Entities.AI;
using Interfaces.AI.UBS.Enemy.LongArms;
using UnityEngine;

namespace AI.Combat.Contexts
{
    public class LongArmsContext : AIEnemyContext, ILongArmsIdleUtility, ILongArmsAcquireNewTargetForThrowRockUtility, 
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
        
        public LongArmsContext(uint totalHealth, float radius, float height, float sightMaximumDistance, float fov,
            Transform agentTransform, AbilityCast throwRockCast, AbilityCast clapAboveCast, float radiusToFlee) : 
            base(EntityType.LONG_ARMS, totalHealth, radius, height, sightMaximumDistance, fov, agentTransform)
        {
            _repeatableActions = new List<uint>
            {
                (uint)LongArmsAction.OBSERVING,
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

        public void SetHasATargetForThrowRock(bool hasATarget)
        {
            _throwRockAbilityHasATarget = hasATarget;
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

            direction = GetAgentTransform().rotation * direction;
            
            return direction;
        }

        public float GetMinimumAngleFromForwardToCastThrowRock()
        {
            return _throwRockCast.minimumAngleToCast;
        }

        public void SetThrowRockTarget(AgentEntity target)
        {
            _throwRockTarget.SetTargetRadius(target.GetRadius());
            _throwRockTarget.SetTargetHeight(target.GetHeight());
            _throwRockTarget.SetTargetTransform(target.GetTransformComponent().GetTransform(),
                GetAgentTransform().position, GetHeight());

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

        public void SetHasATargetForClapAbove(bool hasATarget)
        {
            _clapAboveAbilityHasATarget = hasATarget;
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

            direction = GetAgentTransform().rotation * direction;
            
            return direction;
        }

        public float GetMinimumAngleFromForwardToCastClapAbove()
        {
            return _clapAboveCast.minimumAngleToCast;
        }

        public void SetClapAboveTarget(AgentEntity target)
        {
            _clapAboveTarget.SetTargetRadius(target.GetRadius());
            _clapAboveTarget.SetTargetHeight(target.GetHeight());
            _clapAboveTarget.SetTargetTransform(target.GetTransformComponent().GetTransform(),
                GetAgentTransform().position, GetHeight());

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

        public bool IsSeeingATarget()
        {
            return IsSeeingATargetForThrowRock() || IsSeeingATargetForClapAbove();
        }

        public override bool HasATarget()
        {
            return HasATargetForThrowRock() || HasATargetForClapAbove();
        }
    }
}