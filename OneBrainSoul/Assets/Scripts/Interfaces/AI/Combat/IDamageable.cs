using UnityEngine;

namespace Interfaces.AI.Combat
{
    public interface IDamageable
    {
        public void OnReceiveDamage(uint damageValue, Vector3 hitPosition, Vector3 sourcePosition);
        
        public void OnReceiveDamageOverTime(uint damageValue, float duration, Vector3 sourcePosition);
    }
}