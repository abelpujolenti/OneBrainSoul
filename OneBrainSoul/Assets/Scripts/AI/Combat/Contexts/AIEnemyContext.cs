using System;
using System.Collections.Generic;
using AI.Combat.Enemy;
using Interfaces.AI.UBS.BaseInterfaces.Get;
using Interfaces.AI.UBS.BaseInterfaces.Property;
using UnityEngine;

namespace AI.Combat.Contexts
{
    public abstract class AIEnemyContext : IGetTotalHealth, ILastActionIndex, IHealth, IGetRadius, ITargetRadius, 
        IGetSightMaximumDistance, IDistanceToTarget, IIsSeeingATarget, ITarget, IFighting, IAttacking, IVectorToTarget, 
        ITargetTransform, IGetAgentTransform
    {
        private EnemyType _enemyType;
        
        protected List<uint> _repeatableActions;
        private uint _lastActionIndex = 10;

        private uint _totalHealth;
        private uint _health;

        private float _radius;
        private float _height;
        private float _targetRadius;
        private float _targetHeight;
        private float _sightMaximumDistance;
        private float _distanceToTarget;

        private bool _isSeeingATarget;
        private bool _hasATarget;
        private bool _isFighting;
        private bool _isCastingAnAbility;
        private bool _isAirborne;

        private Vector3 _vectorToTarget;

        private Transform _agentTransform;
        private Transform _targetTransform;

        protected AIEnemyContext(EnemyType enemyType, uint totalHealth, float radius, float height, 
            float sightMaximumDistance, Transform agentTransform)
        {
            _enemyType = enemyType;
            _totalHealth = totalHealth;
            _health = totalHealth;
            _radius = radius;
            _height = height;
            _sightMaximumDistance = sightMaximumDistance != 0 ? sightMaximumDistance : Mathf.Infinity;
            _agentTransform = agentTransform;
        }

        public EnemyType GetEnemyType()
        {
            return _enemyType;
        }

        public List<uint> GetRepeatableActions()
        {
            return _repeatableActions;
        }

        public void SetLastActionIndex(uint lastActionIndex)
        {
            _lastActionIndex = lastActionIndex;
        }

        public uint GetLastActionIndex()
        {
            return _lastActionIndex;
        }

        public uint GetTotalHealth()
        {
            return _totalHealth;
        }

        public void SetHealth(uint health)
        {
            _health = Math.Min(_totalHealth, health);
        }

        public uint GetHealth()
        {
            return _health;
        }

        public float GetRadius()
        {
            return _radius;
        }

        public float GetHeight()
        {
            return _height;
        }

        public void SetTargetRadius(float rivalRadius)
        {
            _targetRadius = rivalRadius;
        }

        public float GetTargetRadius()
        {
            return _targetRadius;
        }

        public void SetTargetHeight(float rivalHeight)
        {
            _targetHeight = rivalHeight;
        }

        public float GetTargetHeight()
        {
            return _targetHeight;
        }

        public float GetSightMaximumDistance()
        {
            return _sightMaximumDistance;
        }

        public void SetDistanceToTarget(float distanceToTarget)
        {
            _distanceToTarget = distanceToTarget;
        }

        public float GetDistanceToTarget()
        {
            return _distanceToTarget;
        }

        public void SetIsSeeingATarget(bool isSeeingATarget)
        {
            _isSeeingATarget = isSeeingATarget;
        }

        public bool IsSeeingATarget()
        {
            return _isSeeingATarget;
        }

        public void SetHasATarget(bool hasATarget)
        {
            _hasATarget = hasATarget;
        }

        public bool HasATarget()
        {
            return _hasATarget;
        }

        public void SetIsFighting(bool isFighting)
        {
            _isFighting = isFighting;
        }

        public bool IsFighting()
        {
            return _isFighting;
        }

        public void SetICastingAnAbility(bool isCastingAnAbility)
        {
            _isCastingAnAbility = isCastingAnAbility;
        }

        public bool IsCastingAnAbility()
        {
            return _isCastingAnAbility;
        }

        public void SetIsAirborne(bool isAirborne)
        {
            _isAirborne = isAirborne;
        }

        public bool IsAirborne()
        {
            return _isAirborne;
        }

        public void SetVectorToTarget(Vector3 vectorToRival)
        {
            _vectorToTarget = vectorToRival;
            SetDistanceToTarget(_vectorToTarget.magnitude - _targetRadius);
        }

        public Vector3 GetVectorToTarget()
        {
            return _vectorToTarget;
        }

        public void SetTargetTransform(Transform targetTransform)
        {
            _targetTransform = targetTransform;

            Vector3 targetPosition = _targetTransform.position;
            targetPosition.y -= _targetHeight / 2;

            Vector3 agentPosition = _agentTransform.position;
            agentPosition.y -= _height / 2;
            
            SetVectorToTarget(targetPosition - agentPosition);

            _hasATarget = true;
        }

        public Transform GetAgentTransform()
        {
            return _agentTransform;
        }

        public Transform GetTargetTransform()
        {
            return _targetTransform;
        }
    }
}