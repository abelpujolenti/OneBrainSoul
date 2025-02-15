using System.Collections;
using Managers;
using UnityEngine;

namespace ECS.Entities.AI.Combat
{
    public class LongArmsBase : AgentEntity
    {
        [SerializeField] private Material _material;
        
        [SerializeField] private uint _totalHealth;
        private uint _health;
        
        [SerializeField] private float _radius;
        [SerializeField] private float _height;
        private Vector3 _offsetToLongArms;

        [SerializeField] private float _agentPositionRadius;
        
        private void Start()
        {
            _health = _totalHealth;

            _offsetToLongArms = new Vector3(0, _height / 2, 0);

            _receiveDamageCooldown = GameManager.Instance.GetEnemyReceiveDamageCooldown();
            
            Setup(_radius + _agentPositionRadius, EntityType.LONG_ARMS_BASE);
            
            CombatManager.Instance.AddEnemy(this);
        }

        public override float GetRadius()
        {
            return _radius;
        }

        public override float GetHeight()
        {
            return _height;
        }

        public override void OnReceiveDamage(uint damageValue, Vector3 hitPosition)
        {
            if (_currentReceiveDamageCooldown > 0f)
            {
                return;
            }
            
            DamageEffect(hitPosition);
            
            _material.SetColor("_DamageColor", new Color(1,0,0));

            _health = (uint)Mathf.Max(0f, _health - damageValue);

            if (_health != 0)
            {
                StartCoroutine(DecreaseDamageCooldown());
                return;
            }

            Destroy(gameObject);
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
            _health = (uint)Mathf.Max(_totalHealth, _health + healValue);
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
        {}

        public override void OnReceiveSlowOverTime(uint slowID, uint slowPercent, float duration)
        {}

        protected override IEnumerator SlowOverTimeCoroutine(uint slowID, uint slowPercent, float duration)
        {
            yield break;
        }

        public override void OnReceiveDecreasingSlow(uint slowID, uint slowPercent, float duration)
        {}

        protected override IEnumerator DecreasingSlowCoroutine(uint slowID, uint slowPercent, float duration, int slow)
        {
            yield break;
        }

        public override void OnReceivePushFromCenter(Vector3 centerPosition, Vector3 forceDirection, float forceStrength)
        {}

        public override void OnReceivePushInADirection(Vector3 colliderForwardVector, Vector3 forceDirection, float forceStrength)
        {}

        public void SetLongArms(LongArms longArms)
        {
            Transform longArmsTransform = longArms.transform;
            longArmsTransform.transform.SetParent(transform);
            longArmsTransform.localPosition = _offsetToLongArms + new Vector3(0, longArms.GetHeight() / 2 ,0);
            
            CombatManager.Instance.RemoveFreeLongArmsBaseId(GetAgentID());
            longArms.SetOnFleeAction(SetFree);
            longArms.SetOnDieAction(GetAgentID);
        }

        private void SetFree()
        {
            CombatManager.Instance.AddFreeLongArmsBaseId(GetAgentID());
        }

        private void OnDestroy()
        {
            CombatManager.Instance.OnEnemyDefeated(this);
        }
    }
}