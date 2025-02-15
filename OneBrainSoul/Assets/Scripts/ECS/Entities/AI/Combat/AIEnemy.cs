using System;
using System.Collections;
using System.Collections.Generic;
using AI.Combat.AbilitySpecs;
using AI.Combat.Contexts;
using AI.Combat.ScriptableObjects;
using Interfaces.AI.UBS.BaseInterfaces.Get;
using Managers;
using UnityEngine;

namespace ECS.Entities.AI.Combat
{
    public abstract class AIEnemy<TContext, TAction> : AgentEntity
        where TContext : AIEnemyContext
        where TAction : Enum
    {
        protected TContext _context;

        protected IGetBestAction<TAction, TContext> _utilityFunction;

        protected Dictionary<TAction, Action> _actions;

        private Coroutine _updateCoroutine;
        
        [SerializeField] protected Material _material;

        [SerializeField] protected Animator _animator;

        [SerializeField] private bool _isMarked;

        private Vector3 _directionToRotate;

        protected float _normalRotationSpeed;
        protected float _currentRotationSpeed;
        
        protected bool _isRotating;

        protected virtual void EnemySetup(float radius, AIEnemyProperties aiEnemyProperties, EntityType entityType)
        {
            _receiveDamageCooldown = GameManager.Instance.GetEnemyReceiveDamageCooldown();

            _currentRotationSpeed = aiEnemyProperties.normalRotationSpeed;
            
            Setup(radius + aiEnemyProperties.agentsPositionRadius, entityType);
            
            InitiateDictionaries();
            
            CreateAbilities();
        }

        protected abstract void CreateAbilities();
        
        #region Context
        
        public abstract TContext GetContext();

        private void SetLastActionIndex(uint lastActionIndex)
        {
            _context.SetLastActionIndex(lastActionIndex);
        }

        protected void SetHealth(uint health)
        {
            _context.SetHealth(health);
        }

        public void SetIsFighting(bool isFighting)
        {
            _context.SetIsFighting(isFighting);
        }

        public void SetIsCastingAnAbility(bool isAttacking)
        {
            _context.SetICastingAnAbility(isAttacking);
        }

        public void SetIsAirborne(bool isAirborne)
        {
            _context.SetIsAirborne(isAirborne);
        }

        protected virtual void CastingAnAbility()
        {
            _context.SetICastingAnAbility(true);
        }

        protected virtual void NotCastingAnAbility()
        {
            _context.SetICastingAnAbility(false);
        }

        #endregion
        
        #region UBS

        protected abstract void InitiateDictionaries();

        protected void CalculateBestAction()
        {
            CheckIfCanPerformGivenAction(_utilityFunction.GetBestAction(_context));
        }

        private void CheckIfCanPerformGivenAction(TAction action)
        {
            uint agentActionUInt = Convert.ToUInt16(action);
            uint lastAction = _context.GetLastActionIndex();

            List<uint> repeatableActions = _context.GetRepeatableActions();

            if (agentActionUInt == lastAction && !repeatableActions.Contains(lastAction))
            {
                return;
            }
            
            SetLastActionIndex(agentActionUInt);

            _actions[action]();
        }

        #endregion
        
        #region FSM

        protected abstract void UpdateVisibleTargets();

        protected void Rotate()
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation,
                Quaternion.LookRotation(GetDirectionToRotate()), _currentRotationSpeed * Time.deltaTime);
        }

        protected void SetDirectionToRotate(Vector3 directionToRotate)
        {
            directionToRotate.y = 0;
            _directionToRotate = directionToRotate.normalized;
        }

        protected Vector3 GetDirectionToRotate()
        {
            return _directionToRotate;
        }

        #endregion

        #region Abilities Managing

        #region Own Abilities

        protected IEnumerator StartCooldownCoroutine(AbilityCast abilityCast)
        {
            abilityCast.StartCooldown();
            
            NotCastingAnAbility();

            while (abilityCast.IsOnCooldown())
            {
                abilityCast.DecreaseCooldown();
                yield return null;
            }
        }

        #endregion

        #region Ally Abilities

        public override void OnReceiveHeal(uint healValue)
        {
            SetHealth(_context.GetHealth() + healValue);
        }

        public override void OnReceiveHealOverTime(uint healValue, float duration)
        {
            StartCoroutine(HealOverTimeCoroutine(healValue, duration));
        }

        protected override IEnumerator HealOverTimeCoroutine(uint healValue, float duration)
        {
            float timer = 0;
            float tickTimer = 0;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                tickTimer += Time.deltaTime;

                if (tickTimer >= _timeBetweenHealTicks)
                {
                    OnReceiveHeal(healValue);
                    tickTimer = 0;
                }
                
                yield return null;
            }
        }

        #endregion

        #region Rival Abilities

        public override void OnReceiveDamage(uint damageValue, Vector3 hitPosition)
        {
            if (_currentReceiveDamageCooldown > 0f)
            {
                return;
            }
            
            DamageEffect(hitPosition);
            
            _material.SetColor("_DamageColor", new Color(1,0,0));
            
            SetHealth(_context.GetHealth() - damageValue);

            if (_context.GetHealth() != 0)
            {
                StartCoroutine(DecreaseDamageCooldown());
                return;
            }

            Destroy(gameObject);
        }

        public override void OnReceiveDamageOverTime(uint damageValue, float duration)
        {
            StartCoroutine(DamageOverTimeCoroutine(damageValue, duration));
        }

        protected override IEnumerator DamageOverTimeCoroutine(uint damageValue, float duration)
        {
            float timer = 0;
            float tickTimer = 0;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                tickTimer += Time.deltaTime;

                if (tickTimer >= _timeBetweenDamageTicks)
                {
                    OnReceiveDamage(damageValue, transform.position);
                    tickTimer = 0;
                }
                
                yield return null;
            }
        }

        protected override IEnumerator DecreaseDamageCooldown()
        {
            yield return base.DecreaseDamageCooldown();
            
            _material.SetColor("_DamageColor", new Color(1,1,1));
        }

        public override void OnReceiveSlow(uint slowID, uint slowPercent)
        {
            //TODO ENEMY SLOW
        }

        public override void OnReceiveSlowOverTime(uint slowID, uint slowPercent, float duration)
        {
            //TODO ENEMY SLOW OVER TIME
        }

        protected override IEnumerator SlowOverTimeCoroutine(uint slowID, uint slowPercent, float duration)
        {
            throw new NotImplementedException();
        }

        public override void OnReceiveDecreasingSlow(uint slowID, uint slowPercent, float duration)
        {
            //TODO ENEMY DECREASING SLOW
        }

        protected override IEnumerator DecreasingSlowCoroutine(uint slowID, uint slowPercent, float duration, int slow)
        {
            throw new NotImplementedException();
        }

        protected virtual void OnDestroy()
        {
            if (EventsManager.OnAgentDefeated != null)
            {
                EventsManager.OnAgentDefeated(GetAgentID());
            }

            if (!_isMarked)
            {
                return;
            }
            
            CombatManager.Instance.HealPlayer();
        }

        #endregion

        #endregion

        public override float GetRadius()
        {
            return _context.GetRadius();
        }

        public override float GetHeight()
        {
            return _context.GetHeight();
        }
        
        /////////////////////////DEBUG

        [SerializeField] protected bool _showFov;
        [SerializeField] protected Color _fovColor;

        protected void DrawAbilityCone(Color color, bool hasATarget, AbilityCast abilityCast, Vector3 origin, Vector3 direction,
            int segments)
        {

            float fovAngle = abilityCast.minimumAngleToCast;
            
            if (abilityCast.canCancelCast)
            {
                fovAngle = hasATarget ? abilityCast.maximumAngleToCancelCast : abilityCast.minimumAngleToCast;
            }

            float minimumRange = abilityCast.minimumRangeToCast;
            float maximumRange = abilityCast.maximumRangeToCast;
            
            DrawCone(color, fovAngle, minimumRange, maximumRange, origin, direction, segments);
        }

        protected void DrawCone(Color color, float fovAngle, float minimumRange, float maximumRange, Vector3 origin, Vector3 direction, int segments)
        {
            Gizmos.color = color;

            float maxRadius = Mathf.Tan(fovAngle * Mathf.Deg2Rad * 0.5f) * maximumRange;
            float minRadius = Mathf.Tan(fovAngle * Mathf.Deg2Rad * 0.5f) * minimumRange;

            Vector3 minBaseCenter = origin + direction * minimumRange;
            Vector3 maxBaseCenter = origin + direction * maximumRange;

            Vector3 prevMinPoint = Vector3.zero;
            Vector3 prevMaxPoint = Vector3.zero;
            float angleStep = 360f / segments;

            Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

            for (int i = 0; i <= segments; i++)
            {
                float angle = i * angleStep;
                float rad = angle * Mathf.Deg2Rad;
                Vector3 localOffsetMin = new Vector3(Mathf.Cos(rad) * minRadius, Mathf.Sin(rad) * minRadius, 0);
                Vector3 localOffsetMax = new Vector3(Mathf.Cos(rad) * maxRadius, Mathf.Sin(rad) * maxRadius, 0);

                Vector3 minPoint = minBaseCenter + rotation * localOffsetMin;
                Vector3 maxPoint = maxBaseCenter + rotation * localOffsetMax;

                Gizmos.DrawLine(minPoint, maxPoint);

                if (i > 0)
                {
                    Gizmos.DrawLine(prevMinPoint, minPoint);
                    Gizmos.DrawLine(prevMaxPoint, maxPoint);
                }

                prevMinPoint = minPoint;
                prevMaxPoint = maxPoint;
            }
        }
    }
}