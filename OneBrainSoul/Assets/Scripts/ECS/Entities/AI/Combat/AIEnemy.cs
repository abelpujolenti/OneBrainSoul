using System;
using System.Collections;
using System.Collections.Generic;
using AI.Combat.AbilitySpecs;
using AI.Combat.Contexts;
using AI.Combat.ScriptableObjects;
using Interfaces.AI.UBS.BaseInterfaces.Get;
using Managers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ECS.Entities.AI.Combat
{
    public abstract class AIEnemy<TEnemyProperties, TContext, TAction> : AgentEntity
        where TEnemyProperties : AIEnemyProperties
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

        protected Vector3 _directionOfSight;

        private Vector3 _directionToRotateHead;
        private Vector3 _directionToRotateBody;

        [SerializeField] protected Transform _headTransform; 
        [SerializeField] protected Transform _bodyTransform;

        private float _yawHeadRotation;
        private float _pitchHeadRotation;

        private float _headRotationSpeed;

        protected float _bodyNormalRotationSpeed;
        protected float _bodyCurrentRotationSpeed;

        protected float _minimumTimeInvestigatingArea;
        protected float _maximumTimeInvestigatingArea;

        protected float _minimumTimeInvestigatingAtEstimatedPosition;
        protected float _maximumTimeInvestigatingAtEstimatedPosition;

        protected float _timeInvestigating;

        private uint _maximumHeadPitchUpRotation;
        private uint _maximumHeadPitchDownRotation;
        
        protected bool _isRotating;

        protected virtual void EnemySetup(float radius, TEnemyProperties aiEnemyProperties, EntityType entityType)
        {
            SetDirectionToRotateHead(Vector3.forward);
            
            _receiveDamageCooldown = GameManager.Instance.GetEnemyReceiveDamageCooldown();

            _headRotationSpeed = aiEnemyProperties.headRotationSpeed;

            _bodyNormalRotationSpeed = aiEnemyProperties.bodyNormalRotationSpeed;
            _bodyCurrentRotationSpeed = _bodyNormalRotationSpeed;

            _minimumTimeInvestigatingArea = aiEnemyProperties.minimumTimeInvestigatingArea;
            _maximumTimeInvestigatingArea = aiEnemyProperties.maximumTimeInvestigatingArea;

            _minimumTimeInvestigatingAtEstimatedPosition =
                aiEnemyProperties.minimumTimeInvestigatingAtEstimatedPosition;

            _maximumTimeInvestigatingAtEstimatedPosition =
                aiEnemyProperties.maximumTimeInvestigatingAtEstimatedPosition;

            _maximumHeadPitchUpRotation = aiEnemyProperties.maximumHeadPitchUpRotation;
            _maximumHeadPitchDownRotation = aiEnemyProperties.maximumHeadPitchDownRotation;
            
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

        public void SetIsFSMBlocked(bool isAttacking)
        {
            _context.SetIsFSMBlocked(isAttacking);
        }

        public void SetIsAirborne(bool isAirborne)
        {
            _context.SetIsAirborne(isAirborne);
        }

        protected virtual void BlockFSM()
        {
            _context.SetIsFSMBlocked(true);
        }

        protected virtual void UnblockFSM()
        {
            _context.SetIsFSMBlocked(false);
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

        protected void RotateBody()
        {
            _bodyTransform.rotation = Quaternion.Slerp(_bodyTransform.rotation,
                Quaternion.LookRotation(GetDirectionToRotateBody()), _bodyCurrentRotationSpeed * Time.deltaTime);
        }

        protected void RotateHead()
        {
            _headTransform.rotation = Quaternion.Slerp(_headTransform.rotation,
                Quaternion.LookRotation(_bodyTransform.rotation * GetDirectionToRotateHead()), _headRotationSpeed * Time.deltaTime);
        }

        protected void SetDirectionToRotateHead(Vector3 directionToRotate)
        {
            directionToRotate = directionToRotate.normalized;
            directionToRotate.z = 1;
            _directionToRotateHead = directionToRotate.normalized;
        }

        protected Vector3 GetDirectionToRotateHead()
        {
            return _directionToRotateHead;
        }

        protected Vector3 ReturnDirectionToRotate(float yaw, float pitch)
        {
            float yawRad = Mathf.Deg2Rad * yaw;
            float pitchRad = Mathf.Deg2Rad * pitch;

            return new Vector3(
                Mathf.Cos(pitchRad) * Mathf.Sin(yawRad),
                Mathf.Sin(pitchRad),                   
                Mathf.Cos(pitchRad) * Mathf.Cos(yawRad)
            ).normalized;
        }

        protected void SetDirectionToRotateBody(Vector3 directionToRotate)
        {
            directionToRotate = directionToRotate.normalized;
            directionToRotate.y = 0;
            _directionToRotateBody = directionToRotate.normalized;
        }

        protected Vector3 GetDirectionToRotateBody()
        {
            return _directionToRotateBody;
        }

        protected void OnLoseSightOfTarget(Vector3 lastKnownPosition, Vector3 lastKnownVelocity, float timeElapsed)
        {
            Vector3 estimatedTargetPosition = lastKnownPosition + lastKnownVelocity * timeElapsed;
            
            _timeInvestigating = Random.Range(_minimumTimeInvestigatingAtEstimatedPosition,
                _maximumTimeInvestigatingAtEstimatedPosition);
            
            if (IsInsideSightRange(_headTransform.position, estimatedTargetPosition, _context.GetSightMaximumDistance()))
            {
                Debug.Log("Inside");
                SetDirectionToRotateHead(_headTransform.position - estimatedTargetPosition);
                InvestigateArea();
                return;
            }
            
            Debug.Log("Outside");
            GoToArea(estimatedTargetPosition);
        }

        private bool IsInsideSightRange(Vector3 position, Vector3 targetPosition, float sightDistance)
        {
            Vector3 vectorToTarget = targetPosition - position;
            float distanceToTarget = vectorToTarget.sqrMagnitude;

            if (IsThereAnyObstacleInBetween(position, vectorToTarget.normalized, Mathf.Sqrt(distanceToTarget)))
            {
                return false;
            }

            return !(distanceToTarget > sightDistance * sightDistance);
        }

        protected virtual void InvestigateArea()
        {
            Debug.Log("Investigate");
            Debug.Log(_headTransform.forward);
            Debug.Log(GetDirectionToRotateHead());
            StartCoroutine(InvestigateAreaCoroutine());
        }

        private IEnumerator InvestigateAreaCoroutine()
        {
            float timeLookingAtTheSameDirection = 0.5f;
            
            float timer = 0;
            float timerLookingAtTheSameDirection = 0;

            float timeDeltaTime;

            while (timer < _timeInvestigating)
            {
                if (_context.IsSeeingATarget())
                {
                    break;
                }
                
                timeDeltaTime = Time.deltaTime;
                timer += timeDeltaTime;

                if (Vector3.Dot(_headTransform.forward, GetDirectionToRotateHead()) > 0.95f)
                {
                    timerLookingAtTheSameDirection += timeDeltaTime;

                    if (timerLookingAtTheSameDirection >= timeLookingAtTheSameDirection)
                    {
                        uint maximumHeadYawRotation = _context.GetMaximumHeadYawRotation();
                        float yawHeadRotation = Random.Range(-maximumHeadYawRotation, maximumHeadYawRotation);
                        float pitchHeadRotation = Random.Range(-_maximumHeadPitchDownRotation, _maximumHeadPitchUpRotation);
                        
                        SetDirectionToRotateHead(ReturnDirectionToRotate(yawHeadRotation, pitchHeadRotation));

                        timerLookingAtTheSameDirection = 0;
                    }
                }
                
                yield return null;
            }
            
            OnEndInvestigation();
        }

        protected abstract void GoToArea(Vector3 estimatedPosition);

        protected virtual void OnEndInvestigation()
        {
            SetDirectionToRotateHead(Vector3.forward);
        }

        private bool IsThereAnyObstacleInBetween(Vector3 position, Vector3 direction, float distance)
        {
            return Physics.Raycast(position, direction, distance,
                GameManager.Instance.GetEnemyLayer() + GameManager.Instance.GetGroundLayer());
        }

        #endregion

        #region Abilities Managing

        #region Own Abilities

        protected IEnumerator StartCooldownCoroutine(AbilityCast abilityCast)
        {
            abilityCast.StartCooldown();
            
            UnblockFSM();

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
            //TODO ENEMY SLOW OVER TIME COROUTINE
            yield break;
        }

        public override void OnReceiveDecreasingSlow(uint slowID, uint slowPercent, float duration)
        {
            //TODO ENEMY DECREASING SLOW
        }

        protected override IEnumerator DecreasingSlowCoroutine(uint slowID, uint slowPercent, float duration, int slow)
        {
            //TODO ENEMY DECREASING SLOW COROUTINE
            yield break;
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

            float maxRadius = Mathf.Tan(fovAngle * 2 * Mathf.Deg2Rad * 0.5f) * maximumRange;
            float minRadius = Mathf.Tan(fovAngle * 2 * Mathf.Deg2Rad * 0.5f) * minimumRange;

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