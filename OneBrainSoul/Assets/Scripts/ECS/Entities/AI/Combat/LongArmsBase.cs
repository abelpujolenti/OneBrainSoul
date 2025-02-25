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

        private bool _isFree;
        
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

        public override void OnReceiveDamage(uint damageValue, Vector3 hitPosition, Vector3 sourcePosition)
        {
            if (!_isFree)
            {
                return;
            }
            
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

        public override void OnReceiveDamageOverTime(uint damageValue, float duration, Vector3 sourcePosition)
        {
            if (!_isFree)
            {
                return;
            }
            
            StartCoroutine(DamageOverTimeCoroutine(damageValue, duration, sourcePosition));
        }

        protected override IEnumerator DamageOverTimeCoroutine(uint damageValue, float duration, Vector3 sourcePosition)
        {
            float timer = 0;

            while (timer < duration)
            {
                timer += Time.deltaTime;

                OnReceiveDamage(damageValue, transform.position, sourcePosition);
                
                yield return null;
            }
        }

        public override void OnReceiveHeal(uint healValue, Vector3 sourcePosition)
        {
            _health = (uint)Mathf.Max(_totalHealth, _health + healValue);
        }

        public override void OnReceiveHealOverTime(uint healValue, float duration, Vector3 sourcePosition)
        {
            StartCoroutine(HealOverTimeCoroutine(healValue, duration, sourcePosition));
        }

        protected override IEnumerator HealOverTimeCoroutine(uint healValue, float duration, Vector3 sourcePosition)
        {
            float timer = 0;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                
                OnReceiveHeal(healValue,sourcePosition);

                yield return null;
            }
        }

        public override void OnReceiveSlow(uint slowID, uint slowPercent, Vector3 sourcePosition)
        {}

        public override void OnReceiveSlowOverTime(uint slowID, uint slowPercent, float duration, Vector3 sourcePosition)
        {}

        protected override IEnumerator SlowOverTimeCoroutine(uint slowID, uint slowPercent, float duration, Vector3 sourcePosition)
        {
            yield break;
        }

        public override void OnReceiveDecreasingSlow(uint slowID, uint slowPercent, float duration, Vector3 sourcePosition)
        {}

        protected override IEnumerator DecreasingSlowCoroutine(uint slowID, uint slowPercent, float duration, int slow, Vector3 sourcePosition)
        {
            yield break;
        }

        public override void OnReceivePushFromCenter(Vector3 centerPosition, Vector3 forceDirection, float forceStrength, Vector3 sourcePosition)
        {}

        public override void OnReceivePushInADirection(Vector3 colliderForwardVector, Vector3 forceDirection, float forceStrength, Vector3 sourcePosition)
        {}

        public void SetLongArms(LongArms longArms)
        {
            Transform longArmsTransform = longArms.transform;
            longArmsTransform.transform.SetParent(transform);
            longArmsTransform.localPosition = _offsetToLongArms + new Vector3(0, longArms.GetHeight() / 2 ,0);
            
            CombatManager.Instance.RemoveFreeLongArmsBaseId(GetAgentID());
            longArms.SetOnFleeAction(SetFree);
            longArms.SetLongArmsBaseIdFunc(GetAgentID);
            _isFree = false;
        }

        private void SetFree()
        {
            CombatManager.Instance.AddFreeLongArmsBaseId(GetAgentID());
            _isFree = true;
        }

        public bool IsFree()
        {
            return _isFree;
        }

        private void OnDestroy()
        {
            CombatManager.Instance.OnEnemyDefeated(this);
        }
    }
}