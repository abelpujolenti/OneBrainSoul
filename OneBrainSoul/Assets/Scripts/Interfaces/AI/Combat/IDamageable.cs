using UnityEngine;

namespace Interfaces.AI.Combat
{
    public interface IDamageable
    {
        public void OnReceiveDamage(uint damageValue, Vector3 hitPosition);
    }
}