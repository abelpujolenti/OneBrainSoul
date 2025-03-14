using System;
using UnityEngine;

namespace AI.Combat.AbilitySpecs
{
    public enum AbilityEffectOnHealthType
    {
        DAMAGE,
        HEAL
    }

    public enum AbilityEffectForceType
    {
        FORCE,
        IMPULSE,
        ACCELERATION,
        VELOCITY_CHANGE
    }

    [Serializable]
    public class AbilityEffect
    {
        public bool hasAnEffectOnHealth;
        public uint healthModificationValue;
        public float effectOnHealthDuration;
        public bool isEffectOnHealthAttachedToEntity;

        public bool doesSlow;
        public bool doesDecreaseOverTime;
        public uint slowPercent;
        public float slowDuration;
        public bool isSlowAttachedToEntity;

        public bool doesApplyAForce;
        public Vector3 forceDirection;
        public float forceStrength;
        public bool doesForceComesFromCenterOfTheArea;
        public AbilityEffectForceType abilityEffectForceType;
    }
}