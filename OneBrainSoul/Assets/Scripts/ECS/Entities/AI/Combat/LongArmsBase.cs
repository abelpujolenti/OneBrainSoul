using System;
using System.Collections;
using UnityEngine;

namespace ECS.Entities.AI.Combat
{
    public class LongArmsBase : AgentEntity
    {
        private void Start()
        {
            _entityType = EntityType.LONG_ARMS_BASE;
        }

        public override float GetRadius()
        {
            throw new NotImplementedException();
        }

        public override void OnReceiveDamage(uint damageValue, Vector3 hitPosition)
        {
            //TODO LONG ARMS BASE DAMAGE
        }

        public override void OnReceiveDamageOverTime(uint damageValue, float duration)
        {
            StartCoroutine(DamageOverTimeCoroutine(damageValue, duration));
        }

        protected override IEnumerator DamageOverTimeCoroutine(uint damageValue, float duration)
        {
            float timer = 0;

            while (timer < duration)
            {
                timer += Time.deltaTime;

                OnReceiveDamage(damageValue, transform.position);
                
                yield return null;
            }
        }

        public override void OnReceiveHeal(uint healValue)
        {
            //TODO LONG ARMS BASE HEAL
        }

        public override void OnReceiveHealOverTime(uint healValue, float duration)
        {
            StartCoroutine(HealOverTimeCoroutine(healValue, duration));
        }

        protected override IEnumerator HealOverTimeCoroutine(uint healValue, float duration)
        {
            float timer = 0;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                
                OnReceiveHeal(healValue);

                yield return null;
            }
        }

        public override void OnReceiveSlow(uint slowID, uint slowPercent)
        {
            //TODO LONG ARMS BASE SLOW
        }

        public override void OnReceiveSlowOverTime(uint slowID, uint slowPercent, float duration)
        {
            //TODO LONG ARMS BASE SLOW OVER TIME
        }

        protected override IEnumerator SlowOverTimeCoroutine(uint slowID, uint slowPercent, float duration)
        {
            //TODO LONG ARMS BASE SLOW OVER TIME COROUTINE
            yield break;
        }

        public override void OnReceiveDecreasingSlow(uint slowID, uint slowPercent, float duration)
        {
            //TODO LONG ARMS BASE DECREASING SLOW
        }

        protected override IEnumerator DecreasingSlowCoroutine(uint slowID, uint slowPercent, float duration, int slow)
        {
            //TODO LONG ARMS BASE DECREASING SLOW COROUTINE
            
            yield break;
        }

        public override void OnReceivePushFromCenter(Vector3 centerPosition, Vector3 forceDirection, float forceStrength)
        {}

        public override void OnReceivePushInADirection(Vector3 colliderForwardVector, Vector3 forceDirection, float forceStrength)
        {}
    }
}