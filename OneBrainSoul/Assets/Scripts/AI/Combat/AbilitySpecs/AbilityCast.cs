using System;
using UnityEngine;

namespace AI.Combat.AbilitySpecs
{
    public enum AbilityCastType
    {
        OMNIPRESENT,
        NO_PROJECTILE,
        PARABOLA_PROJECTILE,
        STRAIGHT_LINE_PROJECTILE
    }
    
    [Serializable]
    public class AbilityCast
    {
        public AbilityCast()
        {}

        public AbilityCast(float minimumRangeToCast, float maximumRangeToCast, bool doesSpawnInsideCaster, 
            Vector3 relativeSpawnPosition, bool isAttachedToCaster, float timeToCast, float currentTimeToFinishCast, 
            float cooldown, float currentCooldown)
        {
            this.minimumRangeToCast = minimumRangeToCast;
            this.maximumRangeToCast = maximumRangeToCast;
            this.doesSpawnInsideCaster = doesSpawnInsideCaster;
            this.isAttachedToCaster = isAttachedToCaster;
            this.relativeSpawnPosition = relativeSpawnPosition;
            this.timeToCast = timeToCast;
            _currentTimeToFinishCast = currentTimeToFinishCast;
            this.cooldown = cooldown;
            _currentCooldown = currentCooldown;
        }
        
        public float minimumRangeToCast;
        public float maximumRangeToCast;

        public bool doesSpawnInsideCaster;
        public Vector3 relativeSpawnPosition;

        public bool isAttachedToCaster;

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
            return new AbilityCast(minimumRangeToCast, maximumRangeToCast, doesSpawnInsideCaster,
                relativeSpawnPosition, isAttachedToCaster, timeToCast, _currentTimeToFinishCast,
                cooldown, _currentCooldown);
        }
    }
}