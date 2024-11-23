using System.Collections.Generic;
using AI.Combat.Ally;
using Interfaces.AI.UBS.Ally;
using Interfaces.AI.UBS.BaseInterfaces.Property;
using UnityEngine;

namespace AI.Combat.ScriptableObjects
{
    public class AIAllyContext : AICombatAgentContext, IAllyFollowPlayerUtility, IAllyChooseNewRivalUtility,
        IAllyGetCloserToRivalUtility, IAllyAttackUtility, IAllyFleeUtility, IAllyDodgeAttackUtility, 
        IAllyHelpAnotherMoralGroupUtility, IEnemyStunned
    {
        private uint _oncomingAttackDamage;
        private uint _enemyHealth;

        private float _remainingDistance;
        private float _stoppingDistance;
        private float _height;
        private float _moralWeight;
        private float _threatSuffering;
        private float _minimumPriorityToAttend;
        private float _alertRadius;
        private float _safetyRadius;
        private float _threatWeightOfTarget = 0;
        private float _enemyMaximumStress;
        private float _enemyCurrentStress;

        private bool _canDefeatEnemy;
        private bool _canStunEnemy;
        private bool _isEnemyStunned;
        private bool _isUnderThreat;
        private bool _isUnderAttack;
        private bool _isFleeing;
        private bool _wasRetreatOrderUsed;
        private bool _wasAttackOrderUsed;
        private bool _wasFleeOrderUsed;

        private List<float> _distancesToEnemiesThatThreatMe = new List<float>();
        private Dictionary<uint, float> _groupsHelpPriority = new Dictionary<uint, float>();

        public AIAllyContext(uint totalHealth, uint currentGroup, float radius, float sightMaximumDistance, 
            float minimumRangeToAttack, float maximumRangeToAttack, Transform agentTransform, float stoppingDistance, 
            float height, float moralWeight, float alertRadius, float safetyRadius) : base(totalHealth, 
            currentGroup, radius, sightMaximumDistance, minimumRangeToAttack, maximumRangeToAttack, agentTransform)
        {
            _repeatableActions.Add((uint)AIAllyAction.CHOOSE_NEW_RIVAL);
            _repeatableActions.Add((uint)AIAllyAction.ROTATE);
            _repeatableActions.Add((uint)AIAllyAction.DODGE_ATTACK);
            _repeatableActions.Add((uint)AIAllyAction.ATTACK);

            _stoppingDistance = stoppingDistance;
            _height = height;
            _moralWeight = moralWeight;
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

        public void SetStoppingDistance(float stoppingDistance)
        {
            _stoppingDistance = stoppingDistance;
        }

        public void SetRemainingDistance(float remainingDistance)
        {
            _remainingDistance = remainingDistance;
        }

        public float GetRemainingDistance()
        {
            return _remainingDistance;
        }

        public float GetStoppingDistance()
        {
            return _stoppingDistance;
        }

        public float GetHeight()
        {
            return _height;
        }

        public void SetMoralWeight(float moralWeight)
        {
            _moralWeight = moralWeight;
        }

        public float GetMoralWeight()
        {
            return _moralWeight;
        }

        public void SetThreatSuffering(float threatSuffering)
        {
            _threatSuffering = threatSuffering;
        }

        public float GetThreatSuffering()
        {
            return _threatSuffering;
        }

        public void SetMinimumPriorityToAttend(float minimumPriorityToAttend)
        {
            _minimumPriorityToAttend = minimumPriorityToAttend;
        }

        public float GetMinimumPriorityToAttend()
        {
            return _minimumPriorityToAttend;
        }

        public float GetAlertRadius()
        {
            return _alertRadius;
        }

        public void SetThreatWeightOfTarget(float threatWeightOfTarget)
        {
            _threatWeightOfTarget = threatWeightOfTarget;
        }

        public float GetThreatWeightOfTarget()
        {
            return _threatWeightOfTarget;
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

        public void SetDistancesToEnemiesThatThreatMe(List<float> distancesToThreatGroupsThatThreatMe)
        {
            _distancesToEnemiesThatThreatMe = distancesToThreatGroupsThatThreatMe;
        }

        public List<float> GetDistancesToEnemiesThatThreatMe()
        {
            return _distancesToEnemiesThatThreatMe;
        }

        public void AddGroupToHelp(uint groupID)
        {
            _groupsHelpPriority.Add(groupID, 0);
        }

        public void RemoveGroupToHelp(uint groupID)
        {
            _groupsHelpPriority.Remove(groupID);
        }

        public void SetGroupHelpPriority(uint groupID, float helpPriority)
        {
            _groupsHelpPriority[groupID] = helpPriority;
        }

        public Dictionary<uint, float> GetGroupsHelpPriority()
        {
            return _groupsHelpPriority;
        }

        public override float GetWeight()
        {
            return _moralWeight;
        }
    }
}