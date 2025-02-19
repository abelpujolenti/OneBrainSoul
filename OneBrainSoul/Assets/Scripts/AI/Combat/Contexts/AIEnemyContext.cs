using System;
using System.Collections.Generic;
using ECS.Entities;
using Interfaces.AI.UBS.BaseInterfaces.Get;
using Interfaces.AI.UBS.BaseInterfaces.Property;
using Interfaces.AI.UBS.Enemy;
using UnityEngine;

namespace AI.Combat.Contexts
{
    public abstract class AIEnemyContext : IGetTotalHealth, ILastActionIndex, IHealth, IGetRadius, IGetSightMaximumDistance, 
        IHasATarget, IFighting, ICastingAnAbility, IGetAgentBodyTransform, IEnemyRotationUtility
    {
        private EntityType _entityType;
        
        protected List<uint> _repeatableActions;
        private uint _lastActionIndex = 10;

        private uint _totalHealth;
        private uint _health;
        private uint _maximumHeadYawRotation;

        private float _radius;
        private float _height;
        private float _sightMaximumDistance;
        private uint _fov;

        private bool _isFighting;
        private bool _isFSMBlocked;
        private bool _isAirborne;

        private Transform _headAgentTransform;
        private Transform _bodyAgentTransform;

        protected AIEnemyContext(EntityType entityType, uint totalHealth, uint maximumHeadYawRotation, 
            float radius, float height, float sightMaximumDistance, uint fov, Transform headAgentTransform, Transform bodyAgentTransform)
        {
            _entityType = entityType;
            _totalHealth = totalHealth;
            _health = totalHealth;
            _maximumHeadYawRotation = maximumHeadYawRotation;
            _radius = radius;
            _height = height;
            _sightMaximumDistance = sightMaximumDistance != 0 ? sightMaximumDistance : Mathf.Infinity;
            _fov = fov / 2;
            _headAgentTransform = headAgentTransform;
            _bodyAgentTransform = bodyAgentTransform;
        }

        public EntityType GetEnemyType()
        {
            return _entityType;
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

        public float GetSightMaximumDistance()
        {
            return _sightMaximumDistance;
        }

        public float GetFov()
        {
            return _fov;
        }

        public void SetIsFighting(bool isFighting)
        {
            _isFighting = isFighting;
        }

        public bool IsFighting()
        {
            return _isFighting;
        }

        public void SetIsFSMBlocked(bool isFSMBlocked)
        {
            _isFSMBlocked = isFSMBlocked;
        }

        public bool IsFSMBlocked()
        {
            return _isFSMBlocked;
        }

        public void SetIsAirborne(bool isAirborne)
        {
            _isAirborne = isAirborne;
        }

        public bool IsAirborne()
        {
            return _isAirborne;
        }

        public Transform GetAgentHeadTransform()
        {
            return _headAgentTransform;
        }

        public Transform GetAgentBodyTransform()
        {
            return _bodyAgentTransform;
        }

        public abstract bool IsSeeingATarget();

        public abstract bool HasATarget();
        
        public Vector3 GetVectorToDestination()
        {
            throw new NotImplementedException();
        }

        public uint GetMaximumHeadYawRotation()
        {
            return _maximumHeadYawRotation;
        }
    }
}