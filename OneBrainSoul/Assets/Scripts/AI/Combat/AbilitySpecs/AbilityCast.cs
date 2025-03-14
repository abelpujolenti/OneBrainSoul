using System;
using UnityEngine;

namespace AI.Combat.AbilitySpecs
{
    [Serializable]
    public class AbilityCast
    {
        public AbilityCast()
        {}

        public AbilityCast(bool canCancelCast, float timeToCast, float currentTimeToFinishCast, 
            float cooldown, float currentCooldown)
        {
            this.canCancelCast = canCancelCast;
            this.timeToCast = timeToCast;
            _currentTimeToFinishCast = currentTimeToFinishCast;
            this.cooldown = cooldown;
            _currentCooldown = currentCooldown;
        }

        public bool canCancelCast;

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
            return new AbilityCast(canCancelCast, timeToCast, _currentTimeToFinishCast, cooldown, _currentCooldown);
        }
    }
}