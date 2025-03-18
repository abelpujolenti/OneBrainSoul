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
        private HashSet<uint> _targetsInsideSlamDetectionArea = new HashSet<uint>();
        private TargetContext _slamTarget = new TargetContext();
        
        public TrifaceContext(uint totalHealth,float radius, float height, 
            Transform headAgentTransform, Transform bodyAgentTransform, AbilityCast slamCast) : 
            base(EntityType.TRIFACE, totalHealth,radius, height, headAgentTransform, bodyAgentTransform)
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

        public bool IsSlamOnCooldown()
        {
            return _slamCast.IsOnCooldown();
        }

        public TargetContext GetSlamTargetContext()
        {
            return _slamTarget;
        }

        public void AddTargetInsideSlamDetectionArea(uint targetId)
        {
            _targetsInsideSlamDetectionArea.Add(targetId);
        }

        public void RemoveTargetInsideSlamDetectionArea(uint targetId)
        {
            _targetsInsideSlamDetectionArea.Remove(targetId);
        }

        public bool IsSlamTargetInsideDetectionArea()
        {
            return _targetsInsideSlamDetectionArea.Contains(_slamTarget.GetTargetId());
        }

        public void SetSlamTargetProperties(uint targetId, float targetRadius, float targetHeight)
        {
            SetIsFighting(true);
            _slamTarget.SetTargetProperties(targetId, targetRadius, targetHeight);

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