using System.Collections.Generic;
using AI.Combat.Ally;
using Interfaces.AI.UBS.Ally;
using Interfaces.AI.UBS.BaseInterfaces.Property;
using UnityEngine;

namespace AI.Combat.ScriptableObjects
{
    public class AIAllyContext : AICombatAgentContext, IAllyFollowPlayerUtility, IAllyChooseNewRivalUtility,
        IAllyGetCloserToRivalUtility, IAllyAttackUtility, IAllyFleeUtility, IAllyDodgeAttackUtility, IEnemyStunned
    {
        private uint _oncomingAttackDamage;
        private uint _enemyHealth;
        private uint _minimumEnemiesInsideAlertRadiusToFlee;

        private float _remainingDistance;
        private float _threatSuffering;
        private float _alertRadius;
        private float _safetyRadius;
        private float _enemyMaximumStress;
        private float _enemyCurrentStress;
        private float _timeToNextEnemyMeleeAttack;
        private float _distanceToCloserEnemyProjectile;

        private bool _canDefeatEnemy;
        private bool _canStunEnemy;
        private bool _isEnemyStunned;
        private bool _isUnderThreat;
        private bool _isUnderAttack;
        private bool _isFleeing;
        private bool _wasRetreatOrderUsed;
        private bool _wasAttackOrderUsed;
        private bool _wasFleeOrderUsed;

        private List<Vector3> _vectorToEnemiesThatThreatMe = new List<Vector3>();
        private List<float> _distancesToEnemiesThatTargetsMe = new List<float>();

        public AIAllyContext(uint totalHealth, float radius, float height, float sightMaximumDistance, float minimumRangeToAttack, 
            float maximumRangeToAttack, Transform agentTransform, uint minimumEnemiesInsideAlertRadiusToFlee, 
            float alertRadius, float safetyRadius) : base(totalHealth, radius, height, sightMaximumDistance, 
            minimumRangeToAttack, maximumRangeToAttack, agentTransform)
        {
            _repeatableActions.Add((uint)AIAllyAction.CHOOSE_NEW_RIVAL);
            _repeatableActions.Add((uint)AIAllyAction.ROTATE);
            _repeatableActions.Add((uint)AIAllyAction.ATTACK);
            _repeatableActions.Add((uint)AIAllyAction.FLEE);
            _repeatableActions.Add((uint)AIAllyAction.DODGE_ATTACK);

            _minimumEnemiesInsideAlertRadiusToFlee = minimumEnemiesInsideAlertRadiusToFlee;

            _alertRadius = alertRadius;
            _safetyRadius = safetyRadius;
        }

        public void SetRivalHealth(uint rivalHealth)
        {
            _enemyHealth = rivalHealth;
        }

        public uint GetRivalHealth()
        {
            return _enemyHealth;
        }

        public uint GetMinimumEnemiesAroundToFlee()
        {
            return _minimumEnemiesInsideAlertRadiusToFlee;
        }

        public void SetRemainingDistance(float remainingDistance)
        {
            _remainingDistance = remainingDistance;
        }

        public float GetRemainingDistance()
        {
            return _remainingDistance;
        }

        public void SetThreatSuffering(float threatSuffering)
        {
            _threatSuffering = threatSuffering;
        }

        public float GetThreatSuffering()
        {
            return _threatSuffering;
        }

        public float GetAlertRadius()
        {
            return _alertRadius;
        }

        public float GetSafetyRadius()
        {
            return _safetyRadius;
        }

        public void SetRivalMaximumStress(float rivalMaximumStress)
        {
            _enemyMaximumStress = rivalMaximumStress;
        }

        public float GetRivalMaximumStress()
        {
            return _enemyMaximumStress;
        }

        public void SetRivalCurrentStress(float rivalCurrentStress)
        {
            _enemyCurrentStress = rivalCurrentStress;
        }

        public float GetRivalCurrentStress()
        {
            return _enemyCurrentStress;
        }

        public void SetTimeToNextEnemyMeleeAttack(float timeToNextEnemyMeleeAttack)
        {
            _timeToNextEnemyMeleeAttack = timeToNextEnemyMeleeAttack;
        }

        public float GetTimeToNextEnemyMeleeAttack()
        {
            return _timeToNextEnemyMeleeAttack;
        }

        public void SetDistanceToCloserEnemyProjectile(float distanceToCloserEnemyProjectile)
        {
            _distanceToCloserEnemyProjectile = distanceToCloserEnemyProjectile;
        }

        public float GetDistanceToCloserEnemyProjectile()
        {
            return _distanceToCloserEnemyProjectile;
        }

        public void SetCanDefeatEnemy(bool canDefeatEnemy)
        {
            _canDefeatEnemy = canDefeatEnemy;
        }

        public bool CanDefeatEnemy()
        {
            return _canDefeatEnemy;
        }

        public void SetCanStunEnemy(bool canStunEnemy)
        {
            _canStunEnemy = canStunEnemy;
        }

        public bool CanStunEnemy()
        {
            return _canStunEnemy;
        }

        public void SetIsEnemyStunned(bool isEnemyStunned)
        {
            _isEnemyStunned = isEnemyStunned;
        }

        public bool IsEnemyStunned()
        {
            return _isEnemyStunned;
        }

        public void SetIsUnderThreat(bool isUnderThreat)
        {
            _isUnderThreat = isUnderThreat;
        }

        public bool IsUnderThreat()
        {
            return _isUnderThreat;
        }

        public void SetIsUnderAttack(bool isUnderAttack)
        {
            _isUnderAttack = isUnderAttack;
        }

        public bool IsUnderAttack()
        {
            return _isUnderAttack;
        }

        public bool IsFleeing()
        {
            return _isFleeing;
        }

        public void SetOncomingAttackDamage(uint oncomingAttackDamage)
        {
            _oncomingAttackDamage = oncomingAttackDamage;
        }

        public uint GetOncomingAttackDamage()
        {
            return _oncomingAttackDamage;
        }

        public void SetIsInRetreatState(bool isInRetreatState)
        {
            _wasRetreatOrderUsed = isInRetreatState;
        }

        public bool IsInRetreatState()
        {
            return _wasRetreatOrderUsed;
        }

        public void SetIsInAttackState(bool isInAttackState)
        {
            _wasAttackOrderUsed = isInAttackState;
        }

        public bool IsInAttackState()
        {
            return _wasAttackOrderUsed;
        }

        public void SetIsInFleeState(bool isInFleeState)
        {
            _wasFleeOrderUsed = isInFleeState;
        }

        public bool IsInFleeState()
        {
            return _wasFleeOrderUsed;
        }
        
        public void SetVectorsToEnemiesThatTargetsMe(List<Vector3> vectorsToEnemiesThatTargetsMe)
        {
            _vectorToEnemiesThatThreatMe = vectorsToEnemiesThatTargetsMe;
        }

        public List<Vector3> GetVectorsToEnemiesThatTargetsMe()
        {
            return _vectorToEnemiesThatThreatMe;
        }
        
        public void SetDistancesToEnemiesThatTargetsMe(List<float> distancesToEnemiesThatTargetsMe)
        {
            _distancesToEnemiesThatTargetsMe = distancesToEnemiesThatTargetsMe;
        }

        public List<float> GetDistancesToEnemiesThatTargetsMe()
        {
            return _distancesToEnemiesThatTargetsMe;
        }
    }
}