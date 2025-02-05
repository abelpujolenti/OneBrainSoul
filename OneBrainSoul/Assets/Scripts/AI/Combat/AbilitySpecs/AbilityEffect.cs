using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace AI.Combat.AbilitySpecs
{
    public enum AbilityEffectOnHealthType
    {
        DAMAGE,
        HEAL
    }

    public enum AbilityEffectForceApplicationType
    {
        CENTER,
        DIRECTIONAL
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
        [FormerlySerializedAs("doesForceComesFromCenter")] public bool doesForceComesFromCenterOfTheArea;
    }
}