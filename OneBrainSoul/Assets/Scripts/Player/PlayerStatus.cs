using System;
using ECS.Entities.AI;
using Managers;
using UnityEngine;

namespace Player
{
    public class PlayerStatus : AgentEntity
    {
        [SerializeField] private float _agentsPositionRadius;
        
        private float _height;
        private float _radius;

        private void Start()
        {
            _receiveDamageCooldown = GameManager.Instance.GetPlayerReceiveDamageCooldown();
            
            CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();

            _height = capsuleCollider.height;
            _radius = capsuleCollider.radius;
            
            Setup(_radius + _agentsPositionRadius);
            
            CombatManager.Instance.AddPlayer(this);
        }

        public override void OnReceiveDamage(uint damageValue, Vector3 hitPosition)
        {
            if (_currentReceiveDamageCooldown > 0f)
            {
                return;
            }
            
            Debug.Log("Player Damage : " + damageValue);
            
            base.OnReceiveDamage(damageValue, hitPosition);
            
            StartCoroutine(DecreaseDamageCooldown());
            //TODO MANAGE PLAYER HEALTH
        }

        public override void OnReceiveDamageOverTime(uint damageValue, float duration)
        {
            base.OnReceiveDamageOverTime(damageValue, duration);
            
            Debug.Log("Player Damage Over Time: " + damageValue + " - Duration: " + duration);
        }

        public override void OnReceiveHeal(uint healValue)
        {
            Debug.Log("Player Heal: " + healValue);
        }

        public override void OnReceiveHealOverTime(uint healValue, float duration)
        {
            Debug.Log("Player Heal Over Time: " + healValue + " - Duration: " + duration);
        }

        public override void OnReceiveSlow(uint slowPercent)
        {
            Debug.Log("Player Slow: " + slowPercent);
        }

        public override void OnReceiveSlowOverTime(uint slowPercent, float duration)
        {
            Debug.Log("Player Slow Over Time: " + slowPercent + " - Duration: " + duration);
        }

        public override void OnReceiveDecreasingSlow(uint slowPercent, float duration)
        {
            Debug.Log("Player Decreasing Slow: " + slowPercent + " - Duration: " + duration);
        }

        public override void OnReleaseFromSlow()
        {
            base.OnReleaseFromSlow();
            
            Debug.Log("Player Release From Slow");
        }

        public override void OnReceivePush(Vector3 forceDirection, float forceStrength)
        {
            base.OnReceivePush(forceDirection, forceStrength);
            
            Debug.Log("Player Push: " + forceDirection + " - Strength: " + forceStrength);
        }

        public float GetHeight()
        {
            return _height;
        }

        public float GetRadius()
        {
            return _radius;
        }
    }
}