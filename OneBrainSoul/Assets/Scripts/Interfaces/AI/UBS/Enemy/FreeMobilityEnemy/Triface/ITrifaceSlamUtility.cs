using AI.Combat.Contexts.Target;
using Interfaces.AI.UBS.Enemy.FreeMobilityEnemy.Triface.BaseInterfaces;
using UnityEngine;

namespace Interfaces.AI.UBS.Enemy.FreeMobilityEnemy.Triface
{
    public interface ITrifaceSlamUtility : ITrifaceHasATargetForSlam
    {
        public bool IsSlamOnCooldown();
        
        public float GetSlamMinimumRangeToCast();
        
        public float GetSlamMaximumRangeToCast();

        public TargetContext GetSlamTargetContext();

        public Vector3 GetDirectionOfSlamDetection();

        public float GetMinimumAngleFromForwardToCastSlam();
    }
}