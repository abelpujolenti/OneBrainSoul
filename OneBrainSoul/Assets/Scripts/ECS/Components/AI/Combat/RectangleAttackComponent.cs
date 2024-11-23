﻿using AI.Combat;
using AI.Combat.ScriptableObjects;
using UnityEngine;

namespace ECS.Components.AI.Combat
{
    public class RectangleAttackComponent : AttackComponent
    {
        private Vector3 _direction;
        private float _length;
        private float _wideness;

        public RectangleAttackComponent(AIAttack aiAttack) : base(aiAttack)
        {
            AIAttackAoE aiAttackAoE = aiAttack.attackAoE;
            
            _direction = aiAttackAoE.GetDirection();
            _length = aiAttackAoE.GetLength();
            _wideness = aiAttackAoE.GetWideness();
        }

        public Vector3 GetDirection()
        {
            return _direction;
        }

        public float GetLength()
        {
            return _length;
        }

        public float GetWidth()
        {
            return _wideness;
        }
    }
}