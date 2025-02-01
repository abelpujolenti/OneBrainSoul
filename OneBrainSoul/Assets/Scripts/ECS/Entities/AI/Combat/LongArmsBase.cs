using UnityEngine;

namespace ECS.Entities.AI.Combat
{
    public class LongArmsBase : AgentEntity
    {
        private void Start()
        {
            _entityType = EntityType.LONG_ARMS_BASE;
        }

        public override void OnReceiveDamage(uint damageValue, Vector3 hitPosition)
        {
            //TODO LONG ARMS BASE DAMAGE
        }

        public override void OnReceiveHeal(uint healValue)
        {
            //TODO LONG ARMS BASE HEAL
        }

        public override void OnReceiveHealOverTime(uint healValue, float duration)
        {
            //TODO LONG ARMS BASE HEAL
        }

        public override void OnReceiveSlow(uint slowPercent)
        {
            //TODO LONG ARMS BASE SLOW
        }

        public override void OnReceiveSlowOverTime(uint slowPercent, float duration)
        {
            //TODO LONG ARMS BASE SLOW OVER TIME
        }

        public override void OnReceiveDecreasingSlow(uint slowPercent, float duration)
        {
            //TODO LONG ARMS BASE DECREASING SLOW
        }

        public override void OnReceivePush(Vector3 forceDirection, float forceStrength)
        {
            //TODO LONG ARMS BASE DAMAGE
        }
    }
}