using System;
using System.Collections;
using System.Collections.Generic;
using AI;
using AI.Combat.CombatNavigation;
using AI.Combat.ScriptableObjects;
using AI.Combat.Steering;
using ECS.Components.AI.Combat;
using ECS.Components.AI.Navigation;
using ECS.Entities.AI.Navigation;
using Managers;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace ECS.Entities.AI.Combat
{
    public abstract class AICombatAgentEntity<TContext, TAttackComponent, TDamageComponent> : NavMeshAgentEntity 
        where TContext : AICombatAgentContext
        where TAttackComponent : AttackComponent
        where TDamageComponent : DamageComponent
    {
        private List<Node> TESTpath;

        protected TContext _context;

        protected List<TAttackComponent> _attackComponents = new List<TAttackComponent>();

        protected List<uint> _visibleRivals = new List<uint>();

        protected DefeatComponent _defeatComponent;

        protected VectorComponent _lastDestination;

        private Coroutine _updateCoroutine;

        protected float _minimumRangeToCastAnAttack;
        protected float _maximumRangeToCastAnAttack;

        protected DirectionWeights[] _raysDirectionAndWeights;
        
        protected float _raysOpeningAngle = 90f;
        protected float _raysDistance = 20f;
        
        protected uint _numberOfVicinityRays = 12;

        protected int _raysTargetsLayerMask;

        protected virtual void StartUpdate()
        {
            if (_updateCoroutine != null)
            {
                return;
            }
            _updateCoroutine = StartCoroutine(UpdateCoroutine());
        }

        protected virtual void StopUpdate()
        {
            StopCoroutine(_updateCoroutine);
            _updateCoroutine = null;
        }

        protected abstract IEnumerator UpdateCoroutine();

        protected override IEnumerator RotateToGivenPositionCoroutine(Vector3 position)
        {
            yield return base.RotateToGivenPositionCoroutine(position);
        }

        protected void SetRaysDirections()
        {
            _raysDirectionAndWeights = new DirectionWeights[_numberOfVicinityRays];
            
            float angle = -(_raysOpeningAngle / 2);
            float angleStep = _raysOpeningAngle / _numberOfVicinityRays;

            Transform ownTransform = transform;

            for (int i = 0; i < _numberOfVicinityRays; i++)
            {
                Vector3 direction = Quaternion.Euler(0, angle, 0) * ownTransform.forward;
                _raysDirectionAndWeights[i].direction = direction.normalized;
                angle += angleStep;
            }
        }

        protected void LaunchRaycasts()
        {
            SetRaysDirections();
            
            Vector3 position = transform.position;

            RaycastHit hit;
            
            for (int i = 0; i < _numberOfVicinityRays; i++)
            {
                if (Physics.Raycast(position, _raysDirectionAndWeights[i].direction, out hit, _raysDistance, _raysTargetsLayerMask))
                {
                    _raysDirectionAndWeights[i].weight = MathUtil.Map(hit.distance, 0, 1, _raysDistance, 0);
                    //Debug.Log(hit.collider.name);
                    continue;
                }

                _raysDirectionAndWeights[i].weight = 0;
            }
        }

        protected void CalculateMinimumAndMaximumRangeToAttacks(List<TAttackComponent> attacks)
        {
            _minimumRangeToCastAnAttack = attacks[0].GetMinimumRangeCast();
            _maximumRangeToCastAnAttack = attacks[0].GetMaximumRangeCast();

            for (int i = 1; i < attacks.Count; i++)
            {
                TAttackComponent attack = attacks[i];
                
                float minimumAttackRange = attack.GetMinimumRangeCast();
                float maximumAttackRange = attack.GetMaximumRangeCast();

                if (minimumAttackRange < _minimumRangeToCastAnAttack)
                {
                    _minimumRangeToCastAnAttack = minimumAttackRange;
                }

                if (maximumAttackRange > _maximumRangeToCastAnAttack)
                {
                    _maximumRangeToCastAnAttack = maximumAttackRange;
                }
            }
        }

        protected TAttackComponent ReturnNextAttack()
        {
            List<TAttackComponent> possibleAttacks = new List<TAttackComponent>();
            
            List<float> minimumRangesInsideCurrentRange = new List<float>();
            List<float> maximumRangesInsideCurrentRange = new List<float>();

            float currentMinimumRangeToAttack = _context.GetMinimumRangeToAttack();
            float currentMaximumRangeToAttack = _context.GetMaximumRangeToAttack();
            
            foreach (TAttackComponent attackComponent in _attackComponents)
            {
                float currentAttackMinimumRangeToCast = attackComponent.GetMinimumRangeCast();
                float currentAttackMaximumRangeToCast = attackComponent.GetMaximumRangeCast();
                
                if (currentAttackMinimumRangeToCast < currentMinimumRangeToAttack ||
                    currentAttackMaximumRangeToCast > currentMaximumRangeToAttack ||
                    attackComponent.IsOnCooldown())
                {
                    continue;
                }
                
                minimumRangesInsideCurrentRange.Add(currentAttackMinimumRangeToCast);
                maximumRangesInsideCurrentRange.Add(currentAttackMaximumRangeToCast);
                
                possibleAttacks.Add(attackComponent);
            }

            int randomNumber = Random.Range(0, possibleAttacks.Count);

            TAttackComponent selectedAttackComponent = possibleAttacks[randomNumber];
            
            minimumRangesInsideCurrentRange.RemoveAt(randomNumber);
            maximumRangesInsideCurrentRange.RemoveAt(randomNumber);

            if (minimumRangesInsideCurrentRange.Count == 0)
            {
                _context.SetMinimumRangeToAttack(_maximumRangeToCastAnAttack);
                _context.SetMaximumRangeToAttack(_minimumRangeToCastAnAttack);
                return selectedAttackComponent;
            }

            if (Math.Abs(selectedAttackComponent.GetMinimumRangeCast() - currentMinimumRangeToAttack) < 0.3f)
            {
                UpdateMinimumRangeToCast(minimumRangesInsideCurrentRange);
            }

            if (Math.Abs(selectedAttackComponent.GetMaximumRangeCast() - currentMaximumRangeToAttack) < 0.3f)
            {
                UpdateMaximumRangeToCast(maximumRangesInsideCurrentRange);
            }

            return selectedAttackComponent;
        }

        protected void Attacking()
        {
            _context.SetIsAttacking(true);
            _navMeshAgent.isStopped = true;
        }

        public void NotAttacking()
        {
            _context.SetIsAttacking(false);
            _navMeshAgent.isStopped = false;
        }

        public virtual void OnAttackAvailableAgain(TAttackComponent attackComponent)
        {
            float attackMinimumRangeToCast = attackComponent.GetMinimumRangeCast();
            float attackMaximumRangeToCast = attackComponent.GetMaximumRangeCast();

            if (_context.GetMinimumRangeToAttack() > attackMinimumRangeToCast)
            {
                _context.SetMinimumRangeToAttack(attackMinimumRangeToCast);
            }

            if (_context.GetMaximumRangeToAttack() > attackMaximumRangeToCast)
            {
                return;
            }
            
            _context.SetMaximumRangeToAttack(attackMaximumRangeToCast);
        }

        public void SetLastActionIndex(uint lastActionIndex)
        {
            _context.SetLastActionIndex(lastActionIndex);
        }

        public void SetHealth(uint health)
        {
            _context.SetHealth(health);
        }

        public void SetRivalIndex(uint rivalIndex)
        {
            _context.SetRivalIndex(rivalIndex);
        }

        public void SetRivalRadius(float rivalRadius)
        {
            _context.SetRivalRadius(rivalRadius);
        }

        public void SetDistanceToRival(float distanceToRival)
        {
            _context.SetDistanceToRival(distanceToRival);
        }

        public void SetIsSeeingARival(bool isSeeingARival)
        {
            _context.SetIsSeeingARival(isSeeingARival);
        }

        public void SetHasATarget(bool hasATarget)
        {
            _context.SetHasATarget(hasATarget);
        }

        public void SetIsFighting(bool isFighting)
        {
            _context.SetIsFighting(isFighting);
        }

        public void SetIsAttacking(bool isAttacking)
        {
            _context.SetIsAttacking(isAttacking);
        }

        public void SetIsAirborne(bool isAirborne)
        {
            _context.SetIsAirborne(isAirborne);
        }

        public void SetVectorToRival(Vector3 vectorToRival)
        {
            _context.SetVectorToRival(vectorToRival);
        }

        public void SetRivalTransform(Transform rivalTransform)
        {
            _context.SetRivalTransform(rivalTransform);
        }

        protected abstract void UpdateVisibleRivals();
        protected abstract void CalculateBestAction();

        public abstract void OnReceiveDamage(TDamageComponent damageComponent);
        protected abstract void OnDefeated();

        public abstract AIAgentType GetAIAgentType();
        
        public abstract TContext GetContext();

        public List<uint> GetVisibleRivals()
        {
            return _visibleRivals;
        }

        public void SetDestination(TransformComponent transformComponent)
        {
            _lastDestination = null;
            ECSNavigationManager.Instance.UpdateNavMeshAgentDestination(GetAgentID(), transformComponent);
        }

        public void SetDestination(VectorComponent vectorComponent)
        {
            _lastDestination = vectorComponent;
            ECSNavigationManager.Instance.UpdateNavMeshAgentDestination(GetAgentID(), _lastDestination);
        }

        protected void UpdateVectorToRival()
        {
            TContext context = GetContext();

            if (!context.HasATarget())
            {
                return;
            }

            Vector3 rivalPosition = context.GetRivalTransform().position;
            
            context.SetVectorToRival(rivalPosition - transform.position);
        }

        private void UpdateMinimumRangeToCast(List<float> minimumRangesInsideCurrentRange)
        {
            float newMinimumRange = minimumRangesInsideCurrentRange[0];

            for (int i = 1; i < minimumRangesInsideCurrentRange.Count; i++)
            {
                float currentMinimumRange = minimumRangesInsideCurrentRange[i];
                
                if (currentMinimumRange > newMinimumRange)
                {
                    continue;
                }

                newMinimumRange = currentMinimumRange;
            }
            
            GetContext().SetMinimumRangeToAttack(newMinimumRange);
        }

        private void UpdateMaximumRangeToCast(List<float> maximumRangesInsideCurrentRange)
        {
            float newMaximumRange = maximumRangesInsideCurrentRange[0];

            for (int i = 1; i < maximumRangesInsideCurrentRange.Count; i++)
            {
                float currentMaximumRange = maximumRangesInsideCurrentRange[i];
                
                if (currentMaximumRange > newMaximumRange)
                {
                    continue;
                }

                newMaximumRange = currentMaximumRange;
            }
            
            GetContext().SetMaximumRangeToAttack(newMaximumRange);
        }

        private void OnDrawGizmos()
        {
            /*Gizmos.color = Color.blue;

            Vector3[] corners = _navMeshAgent.path.corners;

            if (corners.Length == 0)
            {
                return;
            }

            for (int i = 0; i < corners.Length - 1; i++)
            {
                Gizmos.DrawSphere(Up(corners[i]), 0.2f);
                
                Gizmos.DrawLine(Up(corners[i]), Up(corners[i + 1]));
            }
            
            Gizmos.DrawSphere(Up(corners[^1]), 0.2f);
            
            Vector3 position = transform.position;
            
            Gizmos.color = Color.green;
            
            foreach (DirectionWeights directionAndWeight in _raysDirectionAndWeights)
            {
                Gizmos.DrawRay(position, directionAndWeight.direction * _raysDistance);
            }*/
        }

        private Vector3 Up(Vector3 position)
        {
            return new Vector3(position.x, position.y + 0, position.z);
        }
    }
}
