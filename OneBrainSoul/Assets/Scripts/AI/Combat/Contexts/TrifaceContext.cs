using System.Collections.Generic;
using AI.Combat.AbilitySpecs;
using AI.Combat.Contexts.Target;
using AI.Combat.Enemy.Triface;
using ECS.Entities;
using Interfaces.AI.UBS.Enemy.FreeMobilityEnemy.Triface;
using UnityEngine;

namespace AI.Combat.Contexts
{
    public class TrifaceContext : FreeMobilityEnemyContext, ITrifaceIdleUtility, ITrifaceAcquireNewTargetForSlamUtility, 
        ITrifaceSlamUtility
    {
        private bool _isSeeingATargetForSlam;
        private bool _slamAbilityHasATarget;
        private AbilityCast _slamCast;
        private TargetContext _slamTarget = new TargetContext();
        
        public TrifaceContext(uint totalHealth, uint maximumHeadYawRotation,float radius, float height, 
            float sightMaximumDistance, uint fov, Transform headAgentTransform, Transform bodyAgentTransform, AbilityCast slamCast) : 
            base(EntityType.TRIFACE, totalHealth, maximumHeadYawRotation,radius, height, sightMaximumDistance, 
                fov, headAgentTransform, bodyAgentTransform)
        {
            _repeatableActions = new List<uint>
            {
                (uint)TrifaceAction.ROTATE,
                (uint)TrifaceAction.PATROL,
                (uint)TrifaceAction.GO_TO_CLOSEST_SIGHTED_TARGET,
            };

            _slamCast = slamCast;
        }

        public void SetIsSeeingATargetForSlam(bool isSeeingATarget)
        {
            _isSeeingATargetForSlam = isSeeingATarget;
        }

        public bool IsSeeingATargetForSlam()
        {
            return _isSeeingATargetForSlam;
        }

        public bool HasATargetForSlam()
        {
            return _slamAbilityHasATarget;
        }

        public float GetSlamMinimumRangeToCast()
        {
            return _slamCast.minimumRangeToCast;
        }

        public float GetSlamMaximumRangeToCast()
        {
            return _slamCast.maximumRangeToCast;
        }

        public bool IsSlamOnCooldown()
        {
            return _slamCast.IsOnCooldown();
        }

        public TargetContext GetSlamTargetContext()
        {
            return _slamTarget;
        }

        public Vector3 GetDirectionOfSlamDetection()
        {
            Vector3 direction = _slamCast.directionOfDetection;

            direction = GetAgentHeadTransform().rotation * direction;
            
            return direction;
        }

        public float GetMinimumAngleFromForwardToCastSlam()
        {
            return _slamCast.minimumAngleToCast;
        }

        public void SetSlamTargetProperties(float targetRadius, float targetHeight)
        {
            SetIsFighting(true);
            _slamTarget.SetTargetProperties(targetRadius, targetHeight);

            _slamAbilityHasATarget = true;
        }

        public void LoseSlamTarget()
        {
            SetIsFighting(false);
            _slamAbilityHasATarget = false;
        }

        public override bool IsSeeingATarget()
        {
            return IsSeeingATargetForSlam();
        }

        public override bool HasATarget()
        {
            return HasATargetForSlam();
        }
    }
}