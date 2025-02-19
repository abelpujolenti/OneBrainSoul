using System;
using UnityEngine;

namespace AI.Combat.AbilitySpecs
{
    [Serializable]
    public class AbilityCast
    {
        public AbilityCast()
        {}

        public AbilityCast(float minimumRangeToCast, float maximumRangeToCast, Vector3 directionOfDetection, 
            float minimumAngleToCast, bool canCancelCast, float maximumAngleToCancelCast, 
            float timeToCast, float currentTimeToFinishCast, float cooldown, float currentCooldown)
        {
            this.minimumRangeToCast = minimumRangeToCast;
            this.maximumRangeToCast = maximumRangeToCast;
            this.directionOfDetection = directionOfDetection.normalized;
            this.minimumAngleToCast = minimumAngleToCast;
            this.canCancelCast = canCancelCast;
            this.maximumAngleToCancelCast = maximumAngleToCancelCast;
            this.timeToCast = timeToCast;
            _currentTimeToFinishCast = currentTimeToFinishCast;
            this.cooldown = cooldown;
            _currentCooldown = currentCooldown;
        }
        
        public float minimumRangeToCast;
        public float maximumRangeToCast;

        public Vector3 directionOfDetection;
        public float minimumAngleToCast;

        public bool canCancelCast;
        public float maximumAngleToCancelCast;

        public float timeToCast;
        private float _currentTimeToFinishCast;

        public float cooldown;
        private float _currentCooldown;
        
        public float duration;
    
        public void StartCastTime()
        {
            _currentTimeToFinishCast = timeToCast;
        }

        public void DecreaseCurrentCastTime()
        {
            _currentTimeToFinishCast = Mathf.Max(0, _currentTimeToFinishCast - Time.deltaTime);
        }

        public void ResetCastTime()
        {
            _currentTimeToFinishCast = 0;
        }

        public bool IsCasting()
        {
            return _currentTimeToFinishCast != 0;
        }

        public void StartCooldown()
        {
            _currentCooldown = cooldown;
        }

        public void DecreaseCooldown()
        {
            _currentCooldown = Mathf.Max(0, _currentCooldown - Time.deltaTime);
        }

        public bool IsOnCooldown()
        {
            return _currentCooldown != 0;
        }

        public AbilityCast DeepCopy()
        {
            return new AbilityCast(minimumRangeToCast, maximumRangeToCast, directionOfDetection, minimumAngleToCast, 
                canCancelCast, maximumAngleToCancelCast, timeToCast, _currentTimeToFinishCast, cooldown, _currentCooldown);
        }
    }
}