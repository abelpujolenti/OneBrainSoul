using System.Collections.Generic;
using AI.Combat.AbilitySpecs;
using AI.Combat.Contexts.Target;
using AI.Combat.Enemy.Triface;
using ECS.Entities;
using ECS.Entities.AI;
using Interfaces.AI.UBS.Enemy.Triface;
using UnityEngine;

namespace AI.Combat.Contexts
{
    public class TrifaceContext : AIEnemyContext, ITrifaceIdleUtility, ITrifaceAcquireNewTargetForSlamForSlamForSlamUtility,
        ITrifaceGetCloserToTargetOfSlamUtility, ITrifaceSlamUtility
    {
        private bool _isSeeingATargetForSlam;
        private bool _slamAbilityHasATarget;
        private AbilityCast _slamCast;
        private TargetContext _slamTarget = new TargetContext();
        
        public TrifaceContext(uint totalHealth, float radius, float height, float sightMaximumDistance, float fov,
            Transform agentTransform, AbilityCast slamCast) : base(EntityType.TRIFACE, totalHealth, radius, height, 
            sightMaximumDistance, fov, agentTransform)
        {
            _repeatableActions = new List<uint>
            {
                (uint)TrifaceAction.PATROL,
                (uint)TrifaceAction.ROTATE
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

        public void SetHasATargetForSlam(bool hasATarget)
        {
            _slamAbilityHasATarget = hasATarget;
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

            direction = GetAgentTransform().rotation * direction;
            
            return direction;
        }

        public float GetMinimumAngleFromForwardToCastSlam()
        {
            return _slamCast.minimumAngleToCast;
        }

        public void SetSlamTarget(AgentEntity target)
        {
            _slamTarget.SetTargetRadius(target.GetRadius());
            _slamTarget.SetTargetHeight(target.GetHeight());
            _slamTarget.SetTargetTransform(target.GetTransformComponent().GetTransform(), GetAgentTransform().position, 
                GetHeight());

            _slamAbilityHasATarget = true;
        }

        public bool IsSeeingATarget()
        {
            return IsSeeingATargetForSlam();
        }

        public override bool HasATarget()
        {
            return HasATargetForSlam();
        }
    }
}