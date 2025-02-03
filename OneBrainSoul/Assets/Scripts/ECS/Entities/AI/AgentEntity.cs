using System.Collections;
using AI.Combat.Position;
using ECS.Components.AI.Navigation;
using Interfaces.AI.Combat;
using UnityEngine;

namespace ECS.Entities.AI
{
    public abstract class AgentEntity : MonoBehaviour, IDamageable, IHealable, ISlowable, IPushable
    {
        private uint _agentId;

        protected EntityType _entityType;

        private TransformComponent _transformComponent;

        private SurroundingSlots _surroundingSlots;
        protected AgentSlot _agentSlot;

        //[SerializeField] protected Material _material;
        [SerializeField] protected Rigidbody _rigidbody;
        [SerializeField] private GameObject _damageParticlePrefab;

        protected float _receiveDamageCooldown;
        protected float _currentReceiveDamageCooldown;
        
        private ParticleSystem _damageParticle;

        protected void Setup(float agentsPositionRadius)
        {
            Transform ownTransform = transform;
            _agentId = (uint)gameObject.GetInstanceID();
            _transformComponent = new TransformComponent(ownTransform);

            _surroundingSlots = new SurroundingSlots(agentsPositionRadius);
            
            _damageParticle = Instantiate(_damageParticlePrefab, ownTransform.position, Quaternion.identity, ownTransform)
                    .GetComponent<ParticleSystem>();
            
            _damageParticle.gameObject.SetActive(false);
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
        
        public virtual void OnReceiveDamage(uint damageValue, Vector3 hitPosition)
        {
            //TODO DAMAGE POSITION
            //_material.SetColor("_DamageColor", new Color(1,0,0));
            _damageParticle.transform.position = hitPosition;
            _damageParticle.gameObject.SetActive(true);
            _damageParticle.Play();
            AudioManager.instance.PlayOneShot(FMODEvents.instance.enemyDamage, transform.position);
        }

        public virtual void OnReceiveDamageOverTime(uint damageValue, float duration)
        {
            
        }

        protected IEnumerator DecreaseDamageCooldown()
        {
            _currentReceiveDamageCooldown = _receiveDamageCooldown;

            while (_currentReceiveDamageCooldown > 0f)
            {
                _currentReceiveDamageCooldown -= Time.deltaTime;
                yield return null;
            }
            
            //_material.SetColor("_DamageColor", new Color(1,1,1));
        }

        public abstract void OnReceiveHeal(uint healValue);

        public abstract void OnReceiveHealOverTime(uint healValue, float duration);

        public abstract void OnReceiveSlow(uint slowPercent);

        public abstract void OnReceiveSlowOverTime(uint slowPercent, float duration);

        public abstract void OnReceiveDecreasingSlow(uint slowPercent, float duration);

        public virtual void OnReleaseFromSlow()
        {
            //TODO ON RELEASE FROM SLOW
        }

        public virtual void OnReceivePush(Vector3 forceDirection, float forceStrength)
        {
            _rigidbody.AddForce(forceDirection * forceStrength, ForceMode.Acceleration);
        }
    }
}