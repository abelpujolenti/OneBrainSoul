using AI.Combat.Contexts.Target;
using Interfaces.AI.UBS.Enemy.TeleportMobilityEnemy.LongArms.BaseInterfaces;
using UnityEngine;

namespace Interfaces.AI.UBS.Enemy.TeleportMobilityEnemy.LongArms
{
    public interface ILongArmsClapAboveUtility : ILongArmsHasATargetForClapAbove
    {
        public bool IsClapAboveOnCooldown();
        
        public float GetClapAboveMinimRangeToCast();
        
        public float GetClapAboveMaximRangeToCast();

        public TargetContext GetClapAboveTargetContext();

        public Vector3 GetDirectionOfClapAboveDetection();

        public float GetMinimumAngleFromForwardToCastClapAbove();
    }
}