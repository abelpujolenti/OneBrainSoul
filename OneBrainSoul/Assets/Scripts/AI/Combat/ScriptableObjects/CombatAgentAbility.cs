using System;
using AI.Combat.AbilitySpecs;
using UnityEngine;
using UnityEngine.Serialization;

namespace AI.Combat.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Combat Agent Ability Properties", menuName = "ScriptableObjects/AI/Combat/Combat Agent Ability Properties", order = 0)]
    [Serializable]
    public class CombatAgentAbility : ScriptableObject
    {
        public uint totalDamage;

        public float height;

        public bool doesRelativePositionToCasterChange;
        public Vector3 relativePositionToCaster;

        public bool attachToAttacker;

        public bool isRelativePositionXCenterOfColliderX = true;
        public bool isRelativePositionYCenterOfColliderY = true;
        public bool isRelativePositionZCenterOfColliderZ = true;
        
        public float minimumRangeCast;
        public float maximumRangeCast;
        public float timeToCast;
        public bool doesDamageOverTime;
        public float timeDealingDamage;
        public float cooldown;

        public bool itLandsInstantly;

        public float delayBeforeApplyingDamage;

        public Vector3 startRelativePositionToCasterOfTheProjectile;

        public float projectileSpeed;

        public bool doesProjectileExplodeOnAnyContact;
        
        [FormerlySerializedAs("aiAttackAoEType")] public AbilityAoEType abilityAoEType;
        
        public AbilityAoE abilityAoE;
    }
}