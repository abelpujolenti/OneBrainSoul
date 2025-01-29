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
            
            Debug.Log("Player Damage Received: " + damageValue);
            
            base.OnReceiveDamage(damageValue, hitPosition);
            
            StartCoroutine(DecreaseDamageCooldown());
            //TODO MANAGE PLAYER HEALTH
        }
        
        public override void OnReceivePush(Vector3 forceDirection, float forceStrength)
        {
            base.OnReceivePush(forceDirection, forceStrength);
        }

        public override void OnReceiveHeal(uint healValue)
        {
            //TODO PLAYER HEAL
        }

        public override void OnReceiveSlow(uint slowPercent)
        {
            //TODO PLAYER SLOW
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