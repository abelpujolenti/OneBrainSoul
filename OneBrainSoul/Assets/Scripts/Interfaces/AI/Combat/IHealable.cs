using UnityEngine;

namespace Interfaces.AI.Combat
{
    public interface IHealable
    {
        public void OnReceiveHeal(uint healValue, Vector3 sourcePosition);

        public void OnReceiveHealOverTime(uint healValue, float duration, Vector3 sourcePosition);
    }
}