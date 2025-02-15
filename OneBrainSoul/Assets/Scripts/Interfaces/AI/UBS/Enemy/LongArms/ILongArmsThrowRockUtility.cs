using AI.Combat.Contexts.Target;
using Interfaces.AI.UBS.Enemy.LongArms.BaseInterfaces;
using UnityEngine;

namespace Interfaces.AI.UBS.Enemy.LongArms
{
    public interface ILongArmsThrowRockUtility : ILongArmsHasATargetForThrowRock
    {
        public bool IsThrowRockOnCooldown();
        
        public float GetThrowRockMinimRangeToCast();
        
        public float GetThrowRockMaximRangeToCast();

        public TargetContext GetThrowRockTargetContext();

        public Vector3 GetDirectionOfThrowRockDetection();

        public float GetMinimumAngleFromForwardToCastThrowRock();
    }
}