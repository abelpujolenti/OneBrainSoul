using System;
using System.Collections.Generic;
using ECS.Entities;
using Interfaces.AI.UBS.BaseInterfaces.Get;
using Interfaces.AI.UBS.BaseInterfaces.Property;
using UnityEngine;

namespace AI.Combat.Contexts
{
    public abstract class AIEnemyContext : IGetTotalHealth, ILastActionIndex, IHealth, IGetRadius, IGetSightMaximumDistance, 
        IHasATarget, IFighting, ICastingAnAbility, IGetAgentTransform
    {
        private EntityType _entityType;
        
        protected List<uint> _repeatableActions;
        private uint _lastActionIndex = 10;

        private uint _totalHealth;
        private uint _health;

        private float _radius;
        private float _height;
        private float _sightMaximumDistance;
        private float _fov;

        private bool _isFighting;
        private bool _isCastingAnAbility;
        private bool _isAirborne;

        private Transform _agentTransform;

        protected AIEnemyContext(EntityType entityType, uint totalHealth, float radius, float height, 
            float sightMaximumDistance, float fov, Transform agentTransform)
        {
            _entityType = entityType;
            _totalHealth = totalHealth;
            _health = totalHealth;
            _radius = radius;
            _height = height;
            _sightMaximumDistance = sightMaximumDistance != 0 ? sightMaximumDistance : Mathf.Infinity;
            _fov = fov;
            _agentTransform = agentTransform;
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

        public Transform GetAgentTransform()
        {
            return _agentTransform;
        }

        public abstract bool HasATarget();
    }
}