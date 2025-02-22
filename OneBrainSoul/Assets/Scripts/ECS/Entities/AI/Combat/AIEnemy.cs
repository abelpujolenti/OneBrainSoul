using System;
using System.Collections;
using System.Collections.Generic;
using AI.Combat.AbilitySpecs;
using AI.Combat.Contexts;
using AI.Combat.Enemy;
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

        private EntityType _targetEntities;

        [SerializeField] private VisionArea[] _visionAreas;

        protected Dictionary<EntityType, HashSet<uint>> _targetsInsideVisionArea =
            new Dictionary<EntityType, HashSet<uint>>();
        
        [SerializeField] protected Material _material;

        [SerializeField] protected Animator _animator;

        [SerializeField] private bool _isMarked;

        [SerializeField] protected uint _areaNumber;

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

        protected float _timeInvestigating;

        private uint _maximumHeadPitchUpRotation;
        private uint _maximumHeadPitchDownRotation;
        
        protected bool _isRotating;

        protected virtual void EnemySetup(float radius, TEnemyProperties aiEnemyProperties, EntityType entityType, 
            EntityType targetEntities)
        {
            //SetDirectionToRotateHead(Vector3.forward);
            SetDirectionToRotateBody(Vector3.forward);
            
            _receiveDamageCooldown = GameManager.Instance.GetEnemyReceiveDamageCooldown();

            _headRotationSpeed = aiEnemyProperties.headRotationSpeed;

            _bodyNormalRotationSpeed = aiEnemyProperties.bodyNormalRotationSpeed;
            _bodyCurrentRotationSpeed = _bodyNormalRotationSpeed;

            _minimumTimeInvestigatingArea = aiEnemyProperties.minimumTimeInvestigatingArea;
            _maximumTimeInvestigatingArea = aiEnemyProperties.maximumTimeInvestigatingArea;

            _maximumHeadPitchUpRotation = aiEnemyProperties.maximumHeadPitchUpRotation;
            _maximumHeadPitchDownRotation = aiEnemyProperties.maximumHeadPitchDownRotation;
            
            Setup(radius + aiEnemyProperties.agentsPositionRadius, entityType);
            
            InitiateDictionaries();
            
            CreateAbilities();

            foreach (VisionArea visionArea in _visionAreas)
            {
                visionArea.Setup(AddTargetInsideArea, RemoveTargetInsideArea);
            }

            _targetEntities = targetEntities;
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

        protected virtual void InvestigateArea()
        {
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

                if (Vector3.Dot(_headTransform.forward, GetDirectionToRotateBody()) > 0.95f)
                {
                    timerLookingAtTheSameDirection += timeDeltaTime;

                    if (timerLookingAtTheSameDirection >= timeLookingAtTheSameDirection)
                    {
                        uint maximumHeadYawRotation = _context.GetMaximumHeadYawRotation();
                        float yawHeadRotation = Random.Range(-maximumHeadYawRotation, maximumHeadYawRotation);
                        float pitchHeadRotation = Random.Range(-_maximumHeadPitchDownRotation, _maximumHeadPitchUpRotation);
                        
                        SetDirectionToRotateBody(ReturnDirectionToRotate(yawHeadRotation, pitchHeadRotation));

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
            SetDirectionToRotateBody(Vector3.forward);
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

        public override void OnReceiveHeal(uint healValue, Vector3 sourcePosition)
        {
            SetHealth(_context.GetHealth() + healValue);
        }

        public override void OnReceiveHealOverTime(uint healValue, float duration, Vector3 sourcePosition)
        {
            StartCoroutine(HealOverTimeCoroutine(healValue, duration, sourcePosition));
        }

        protected override IEnumerator HealOverTimeCoroutine(uint healValue, float duration, Vector3 sourcePosition)
        {
            float timer = 0;
            float tickTimer = 0;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                tickTimer += Time.deltaTime;

                if (tickTimer >= _timeBetweenHealTicks)
                {
                    OnReceiveHeal(healValue, sourcePosition);
                    tickTimer = 0;
                }
                
                yield return null;
            }
        }

        #endregion

        #region Rival Abilities

        public override void OnReceiveDamage(uint damageValue, Vector3 hitPosition, Vector3 sourcePosition)
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

        public override void OnReceiveDamageOverTime(uint damageValue, float duration, Vector3 sourcePosition)
        {
            StartCoroutine(DamageOverTimeCoroutine(damageValue, duration, sourcePosition));
        }

        protected override IEnumerator DamageOverTimeCoroutine(uint damageValue, float duration, Vector3 sourcePosition)
        {
            float timer = 0;
            float tickTimer = 0;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                tickTimer += Time.deltaTime;

                if (tickTimer >= _timeBetweenDamageTicks)
                {
                    OnReceiveDamage(damageValue, transform.position, sourcePosition);
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

        public override void OnReceiveSlow(uint slowID, uint slowPercent, Vector3 sourcePosition)
        {
            //TODO ENEMY SLOW
        }

        public override void OnReceiveSlowOverTime(uint slowID, uint slowPercent, float duration, Vector3 sourcePosition)
        {
            //TODO ENEMY SLOW OVER TIME
        }

        protected override IEnumerator SlowOverTimeCoroutine(uint slowID, uint slowPercent, float duration, Vector3 sourcePosition)
        {
            //TODO ENEMY SLOW OVER TIME COROUTINE
            yield break;
        }

        public override void OnReceiveDecreasingSlow(uint slowID, uint slowPercent, float duration, Vector3 sourcePosition)
        {
            //TODO ENEMY DECREASING SLOW
        }

        protected override IEnumerator DecreasingSlowCoroutine(uint slowID, uint slowPercent, float duration, int slow, Vector3 sourcePosition)
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

        public EntityType GetTarget()
        {
            return _targetEntities;
        }

        private bool IsADesiredTargetEntity(EntityType entityType)
        {
            return (_targetEntities & entityType) != 0;
        }

        private void AddTargetInsideArea(EntityType entityType, uint targetId)
        {
            if (!IsADesiredTargetEntity(entityType))
            {
                return;
            }
            
            _targetsInsideVisionArea[entityType].Add(targetId);
        }

        private void RemoveTargetInsideArea(EntityType entityType, uint targetId)
        {
            if (!IsADesiredTargetEntity(entityType))
            {
                return;
            }
            
            _targetsInsideVisionArea[entityType].Remove(targetId);
        }

        public override float GetRadius()
        {
            return _context.GetRadius();
        }

        public override float GetHeight()
        {
            return _context.GetHeight();
        }

        public uint GetAreaNumber()
        {
            return _areaNumber;
        }

        /////////////////////////DEBUG

        [SerializeField] protected bool _showFov;
        [SerializeField] protected Color _fovColor;

        protected virtual void OnDrawGizmos()
        {
            if (!_showFov)
            {
                return;
            }
            
            Gizmos.color = _fovColor;
            BoxCollider boxCollider = _visionAreas[0].GetComponent<BoxCollider>();
            Vector3 center = transform.position + transform.rotation * boxCollider.center;
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(center, transform.rotation, Vector3.one);
            Gizmos.DrawCube(Vector3.zero, boxCollider.size);
            Gizmos.matrix = oldMatrix;
            SphereCollider sphereCollider = _visionAreas[1].GetComponent<SphereCollider>();
            Gizmos.DrawSphere(transform.position, sphereCollider.radius);
            //DrawCone(_fovColor, _context.GetFov(), 0, _context.GetSightMaximumDistance(), origin, _headTransform.forward, segments);
        }

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