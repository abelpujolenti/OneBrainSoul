using System.Collections.Generic;
using Interfaces.AI.UBS.BaseInterfaces.Get;
using Interfaces.AI.UBS.BaseInterfaces.Property;
using UnityEngine;

namespace AI.Combat.ScriptableObjects
{
    public abstract class AICombatAgentContext : IGetTotalHealth, ILastActionIndex, IHealth, IRivalIndex, 
        IGetRadius, IRivalRadius, IGetSightMaximumDistance, IDistanceToRival, IMinimumRangeToAttack, IMaximumRangeToAttack, 
        ISeeingARival, ITarget, IFighting, IAttacking, IVectorToRival, IRivalTransform, IGetAgentTransform
    {
        protected List<uint> _repeatableActions = new List<uint>();
        private uint _lastActionIndex = 10;

        private uint _totalHealth;
        private uint _health;
        private uint _rivalIndex;

        private float _radius;
        private float _height;
        private float _rivalRadius;
        private float _rivalHeight;
        private float _sightMaximumDistance;
        private float _distanceToRival;
        private float _minimumRangeToAttack;
        private float _maximumRangeToAttack;

        private bool _isSeeingARival;
        private bool _hasATarget;
        private bool _isFighting;
        private bool _isAttacking;
        private bool _isAirborne;

        private Vector3 _vectorToRival;

        private Transform _agentTransform;
        private Transform _rivalTransform;

        protected AICombatAgentContext(uint totalHealth, float radius, float height, float sightMaximumDistance, float minimumRangeToAttack, 
            float maximumRangeToAttack, Transform agentTransform)
        {
            _totalHealth = totalHealth;
            _health = totalHealth;
            _radius = radius;
            _height = height;
            _sightMaximumDistance = sightMaximumDistance != 0 ? sightMaximumDistance : Mathf.Infinity; 
            _minimumRangeToAttack = minimumRangeToAttack;
            _maximumRangeToAttack = maximumRangeToAttack;
            _agentTransform = agentTransform;
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
            _health = health;
        }

        public uint GetHealth()
        {
            return _health;
        }

        public void SetRivalIndex(uint rivalIndex)
        {
            _rivalIndex = rivalIndex;
        }

        public uint GetRivalID()
        {
            return _rivalIndex;
        }

        public float GetRadius()
        {
            return _radius;
        }

        public float GetHeight()
        {
            return _height;
        }

        public void SetRivalRadius(float rivalRadius)
        {
            _rivalRadius = rivalRadius;
        }

        public float GetRivalRadius()
        {
            return _rivalRadius;
        }

        public void SetRivalHeight(float rivalHeight)
        {
            _rivalHeight = rivalHeight;
        }

        public float GetRivalHeight()
        {
            return _rivalHeight;
        }

        public float GetSightMaximumDistance()
        {
            return _sightMaximumDistance;
        }

        public void SetDistanceToRival(float distanceToRival)
        {
            _distanceToRival = distanceToRival;
        }

        public float GetDistanceToRival()
        {
            return _distanceToRival;
        }

        public void SetMinimumRangeToAttack(float minimumRangeToAttack)
        {
            _minimumRangeToAttack = minimumRangeToAttack;
        }

        public float GetMinimumRangeToAttack()
        {
            return _minimumRangeToAttack;
        }

        public void SetMaximumRangeToAttack(float maximumRangeToAttack)
        {
            _maximumRangeToAttack = maximumRangeToAttack;
        }

        public float GetMaximumRangeToAttack()
        {
            return _maximumRangeToAttack;
        }

        public void SetIsSeeingARival(bool isSeeingARival)
        {
            _isSeeingARival = isSeeingARival;
        }

        public bool IsSeeingARival()
        {
            return _isSeeingARival;
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

        public void SetIsAttacking(bool isAttacking)
        {
            _isAttacking = isAttacking;
        }

        public bool IsAttacking()
        {
            return _isAttacking;
        }

        public void SetIsAirborne(bool isAirborne)
        {
            _isAirborne = isAirborne;
        }

        public bool IsAirborne()
        {
            return _isAirborne;
        }

        public void SetVectorToRival(Vector3 vectorToRival)
        {
            _vectorToRival = vectorToRival;
            SetDistanceToRival(_vectorToRival.magnitude - _rivalRadius);
        }

        public Vector3 GetVectorToRival()
        {
            return _vectorToRival;
        }

        public void SetRivalTransform(Transform rivalTransform)
        {
            _rivalTransform = rivalTransform;

            Vector3 rivalPosition = _rivalTransform.position;
            rivalPosition.y -= _rivalHeight / 2;

            Vector3 agentPosition = _agentTransform.position;
            agentPosition.y -= _height / 2;
            
            SetVectorToRival(rivalPosition - agentPosition);
        }

        public Transform GetRivalTransform()
        {
            return _rivalTransform;
        }

        public Transform GetAgentTransform()
        {
            return _agentTransform;
        }
    }
}