﻿using AI.Combat.Enemy;
using Interfaces.AI.UBS.BaseInterfaces.Property;
using Interfaces.AI.UBS.Enemy;
using UnityEngine;

namespace AI.Combat.ScriptableObjects
{
    public class AIEnemyContext : AICombatAgentContext, IEnemyPatrolUtility, IEnemyChooseNewRivalUtility, 
        IEnemyGetCloserToRivalUtility, IEnemyAttackUtility, IEnemyFleeUtility, IStunned
    {
        private float _maximumStress;
        private float _currentStress;
        private float _stunDuration;

        private bool _isStunned;

        public AIEnemyContext(uint totalHealth, float radius, float sightMaximumDistance, float minimumRangeToAttack, 
            float maximumRangeToAttack, Transform agentTransform, float maximumStress, float stunDuration) : 
            base(totalHealth, radius, sightMaximumDistance, minimumRangeToAttack, maximumRangeToAttack, agentTransform)
        {
            _repeatableActions.Add((uint)AIEnemyAction.CHOOSE_NEW_RIVAL);
            _repeatableActions.Add((uint)AIEnemyAction.ROTATE);
            _repeatableActions.Add((uint)AIEnemyAction.ATTACK);
            
            _maximumStress = maximumStress;
            _stunDuration = stunDuration;
        }

        public float GetMaximumStress()
        {
            return _maximumStress;
        }

        public void SetCurrentStress(float currentStress)
        {
            _currentStress = currentStress;

            if (_currentStress < _maximumStress)
            {
                return;
            }

            _isStunned = true;
            _currentStress = 0;
        }

        public float GetCurrentStress()
        {
            return _currentStress;
        }

        public float GetStunDuration()
        {
            return _stunDuration;
        }

        public void SetIsStunned(bool isStunned)
        {
            _isStunned = isStunned;
        }

        public bool IsStunned()
        {
            return _isStunned;
        }
    }
}