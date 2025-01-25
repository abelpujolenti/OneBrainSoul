using System.Diagnostics;
using UnityEngine;

namespace AI.Combat.AbilityColliders
{
    public abstract class AIEnemyAbilityCollider : AbilityCollider
    {
        private uint _ownerID;

        protected bool _isPlayerInside;

        protected bool _doesInflictedDamageToPlayer;

        protected Stopwatch _stopwatch = new Stopwatch();

        public void SetOwner(uint ownerID)
        {
            _ownerID = ownerID;
        }

        protected override void OnDisable()
        {
            _isPlayerInside = false;
            _doesInflictedDamageToPlayer = false;
        }
        

        public override void StartInflictingDamage()
        {
            if (!_isPlayerInside)
            {
                return;
            }
            
            InflictDamageToPlayer();
        }

        private void InflictDamageToPlayer()
        {
            //TODO ON RECEIVER DAMAGE
            //CombatManager.Instance.RequestPlayer();

            _doesInflictedDamageToPlayer = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_doesInflictedDamageToPlayer)
            {
                return;
            }

            _isPlayerInside = true;
            
            InflictDamageToPlayer();
        }

        private void OnTriggerExit(Collider other)
        {
            _isPlayerInside = false;
        }
    }
}