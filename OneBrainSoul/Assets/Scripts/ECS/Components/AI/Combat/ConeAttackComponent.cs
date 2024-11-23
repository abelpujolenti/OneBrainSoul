﻿using AI.Combat;
using AI.Combat.ScriptableObjects;
using UnityEngine;

namespace ECS.Components.AI.Combat
{
    public class ConeAttackComponent : AttackComponent
    {
        private Vector3 _direction;
        private float _radius;
        private float _degrees;

        public ConeAttackComponent(AIAttack aiAttack) : base(aiAttack)
        {
            AIAttackAoE aiAttackAoE = aiAttack.attackAoE;
            
            _direction = aiAttackAoE.GetDirection();
            _radius = aiAttackAoE.GetRadius();
            _degrees = aiAttackAoE.GetDegrees();
        }

        public Vector3 GetDirection()
        {
            return _direction;
        }

        public float GetRadius()
        {
            return _radius;
        }

        public float GetDegrees()
        {
            return _degrees;
        }
    }
}