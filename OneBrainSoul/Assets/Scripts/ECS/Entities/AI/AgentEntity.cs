using System;
using System.Collections;
using System.Collections.Generic;
using AI.Combat.Position;
using ECS.Components.AI.Navigation;
using Interfaces.AI.Combat;
using Managers;
using UnityEngine;

namespace ECS.Entities.AI
{
    public abstract class AgentEntity : MonoBehaviour, IDamageable, IHealable, ISlowable, IPushable
    {
        private uint _agentId;

        protected EntityType _entityType;

        private TransformComponent _transformComponent;

        private SurroundingSlots _surroundingSlots;

        [SerializeField] protected Rigidbody _rigidbody;
        [SerializeField] private GameObject _damageParticlePrefab;

        protected List<float> _slowEffects;
        protected Dictionary<uint, int> _slowSubscriptions = new Dictionary<uint, int>();

        protected Dictionary<uint, Tuple<int, Coroutine>> _slowCoroutinesSubscriptions =
            new Dictionary<uint, Tuple<int, Coroutine>>();

        protected float _receiveDamageCooldown;
        protected float _currentReceiveDamageCooldown;
        
        private ParticleSystem _damageParticle;

        protected float _timeBetweenDamageTicks;
        protected float _timeBetweenHealTicks;

        protected void Setup(float agentsPositionRadius)
        {
            Transform ownTransform = transform;
            _agentId = (uint)gameObject.GetInstanceID();
            _transformComponent = new TransformComponent(ownTransform);

            _surroundingSlots = new SurroundingSlots(agentsPositionRadius);
            
            _damageParticle = Instantiate(_damageParticlePrefab, ownTransform.position, Quaternion.identity, ownTransform)
                    .GetComponent<ParticleSystem>();
            
            _damageParticle.gameObject.SetActive(false);

            _timeBetweenDamageTicks = GameManager.Instance.GetTimeBetweenDamageTicks();
            _timeBetweenHealTicks = GameManager.Instance.GetTimeBetweenHealTicks();
        }

        public uint GetAgentID()
        {
            return _agentId;
        }

        public TransformComponent GetTransformComponent()
        {
            return _transformComponent;
        }

        public abstract float GetRadius();

        #region Agent Slot
        
        public AgentSlotPosition GetAgentSlotPosition(Vector3 direction, float radius)
        {
            return _surroundingSlots.ReserveSubtendedAngle(GetAgentID(), direction, radius);
        }

        public void ReleaseAgentSlot(uint agentID)
        {
            _surroundingSlots.FreeSubtendedAngle(agentID);
        }

        #endregion

        public abstract void OnReceiveDamage(uint damageValue, Vector3 hitPosition);

        protected void DamageEffect(Vector3 hitPosition)
        {
            _damageParticle.transform.position = hitPosition;
            _damageParticle.gameObject.SetActive(true);
            _damageParticle.Play();
            AudioManager.instance.PlayOneShot(FMODEvents.instance.enemyDamage, transform.position);
        }

        public abstract void OnReceiveDamageOverTime(uint damageValue, float duration);

        protected abstract IEnumerator DamageOverTimeCoroutine(uint damageValue, float duration);

        protected virtual IEnumerator DecreaseDamageCooldown()
        {
            _currentReceiveDamageCooldown = _receiveDamageCooldown;

            while (_currentReceiveDamageCooldown > 0f)
            {
                _currentReceiveDamageCooldown -= Time.deltaTime;
                yield return null;
            }
        }

        public abstract void OnReceiveHeal(uint healValue);

        public abstract void OnReceiveHealOverTime(uint healValue, float duration);

        protected abstract IEnumerator HealOverTimeCoroutine(uint healValue, float duration);

        public abstract void OnReceiveSlow(uint slowID, uint slowPercent);

        public abstract void OnReceiveSlowOverTime(uint slowID, uint slowPercent, float duration);
        
        protected abstract IEnumerator SlowOverTimeCoroutine(uint slowID, uint slowPercent, float duration);

        public abstract void OnReceiveDecreasingSlow(uint slowID, uint slowPercent, float duration);

        protected abstract IEnumerator DecreasingSlowCoroutine(uint slowID, uint slowPercent, float duration, int slow);

        public virtual void OnReleaseFromSlow(uint slowID)
        {
            //TODO ON RELEASE FROM SLOW
        }

        public virtual void OnReceivePushFromCenter(Vector3 centerPosition, Vector3 forceDirection, float forceStrength)
        {
            Vector3 centerToMe = (transform.position - centerPosition).normalized;
            
            OnReceivePushInADirection(centerToMe, forceDirection, forceStrength);
        }

        public virtual void OnReceivePushInADirection(Vector3 colliderForwardVector, Vector3 forceDirection, float forceStrength)
        {
            Vector3 referenceVector = Vector3.up;
            
            if (Vector3.Dot(colliderForwardVector, referenceVector) > 0.99f) 
            {
                referenceVector = Vector3.right;
            }

            Quaternion rotation = Quaternion.LookRotation(colliderForwardVector, referenceVector);
            
            _rigidbody.AddForce(rotation * forceDirection, ForceMode.Impulse);
        }
    }
}