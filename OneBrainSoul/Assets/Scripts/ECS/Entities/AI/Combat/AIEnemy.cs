using System;
using System.Collections;
using System.Collections.Generic;
using AI.Combat.AbilitySpecs;
using AI.Combat.Area;
using AI.Combat.Contexts;
using AI.Combat.ScriptableObjects;
using Interfaces.AI.UBS.BaseInterfaces.Get;
using Managers;
using UnityEngine;

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

        protected HashSet<uint> _targetsSightedInsideCombatArea = new HashSet<uint>();
        
        [SerializeField] protected Material _material;

        [SerializeField] protected Animator _animator;

        [SerializeField] private bool _doesRestoreHealthOfPlayer;
        [SerializeField] private bool _doesRestoreAChargeOfPlayer;

        [SerializeField] protected uint _areaNumber;

        private GameObject _healEffect;

        private float _damageEffectDuration = 0.4f;

        private Vector3 _directionToRotateHead;
        private Vector3 _directionToRotateBody;

        [SerializeField] protected Transform _headTransform; 
        [SerializeField] protected Transform _bodyTransform;

        protected float _bodyNormalRotationSpeed;
        protected float _bodyCurrentRotationSpeed;

        protected float _minimumTimeInvestigatingArea;
        protected float _maximumTimeInvestigatingArea;

        protected float _timeInvestigating;
        
        protected bool _isRotating;

        protected virtual void EnemySetup(float radius, TEnemyProperties aiEnemyProperties, EntityType entityType, 
            EntityType targetEntities)
        {
            //SetDirectionToRotateHead(Vector3.forward);
            SetDirectionToRotateBody(Vector3.forward);
            
            _receiveDamageCooldown = GameManager.Instance.GetEnemyReceiveDamageCooldown();

            _bodyNormalRotationSpeed = aiEnemyProperties.bodyNormalRotationSpeed;
            _bodyCurrentRotationSpeed = _bodyNormalRotationSpeed;

            /*_healEffect = Instantiate(aiEnemyProperties.healEffect, transform);

            _healEffect.transform.localPosition = aiEnemyProperties.healRelativePosition;
            _healEffect.transform.localRotation = Quaternion.Euler(aiEnemyProperties.healRelativeRotation);
            _healEffect.transform.localScale = aiEnemyProperties.healRelativeScale;*/
            
            Setup(radius + aiEnemyProperties.agentsPositionRadius, entityType);
            
            InitiateDictionaries();

            _targetEntities = targetEntities;
            
            CreateAbilities();

            foreach (VisionArea visionArea in _visionAreas)
            {
                visionArea.Setup(AddTargetInsideVisionArea, RemoveTargetInsideVisionArea);
            }
        }

        protected virtual void CreateAbilities()
        {
            for (EntityType i = EntityType.PLAYER; i < EntityType.ENUM_SIZE; i = (EntityType)((int)i << 1))
            {
                if ((_targetEntities & i) == 0)
                {
                    continue;
                }   
                
                _targetsInsideVisionArea.Add(i, new HashSet<uint>());
            }
        }

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

        protected virtual void UpdateSightedTargetsInsideCombatArea()
        {
            _targetsSightedInsideCombatArea =
                CombatManager.Instance.ReturnPositionOfRelevantSightedTargetsInsideCombatArea(_areaNumber, _targetEntities);
            
            _context.SetHasAnyTargetBeenSightedInsideCombatArea(_targetsSightedInsideCombatArea.Count != 0);
        }

        protected abstract void UpdateVisibleTargets();

        protected void RotateBody()
        {
            _bodyTransform.rotation = Quaternion.RotateTowards(_bodyTransform.rotation,
                Quaternion.LookRotation(GetDirectionToRotateBody()), _bodyCurrentRotationSpeed * Time.deltaTime);
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
                        //TODO AI ENEMY ROTATE BODY WHEN INVESTIGATING
                        //SetDirectionToRotateBody(ReturnDirectionToRotate(yawHeadRotation, pitchHeadRotation));

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
                yield return null;
                abilityCast.DecreaseCooldown();
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

            StartCoroutine(DamageEffectCoroutine(_damageEffectDuration));
            
            SetHealth(_context.GetHealth() - damageValue);

            if (_context.GetHealth() != 0)
            {
                StartCoroutine(DecreaseDamageCooldown());
                return;
            }

            StartCoroutine(DelayDeath());
        }

        private IEnumerator DelayDeath()
        {
            float timer = 0;
            
            PreDeath();

            while (timer < _damageEffectDuration)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            Destroy(gameObject);
        }

        protected abstract void PreDeath();

        private IEnumerator DamageEffectCoroutine(float duration)
        {
            Material matInstance = GetComponent<MeshRenderer>().material;

            float t = 0f;
            while (t < duration)
            {
                float p = 1f - t / duration;
                matInstance.SetFloat("_DamageT", Mathf.Pow(p, 0.75f));
                yield return new WaitForFixedUpdate();
                t += Time.fixedDeltaTime;
            }
            matInstance.SetFloat("_DamageT", 0f);
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
            if (EventsManager.OnDefeatEnemy != null)
            {
                EventsManager.OnDefeatEnemy();
            }

            if (EventsManager.OnAgentDefeated != null)
            {
                EventsManager.OnAgentDefeated(GetEntityType(), GetAgentID());
            }

            if (_doesRestoreAChargeOfPlayer)
            {
                CombatManager.Instance.RechargeAChargeOfPlayer();
            }

            if (!_doesRestoreHealthOfPlayer)
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

        private void AddTargetInsideVisionArea(EntityType entityType, uint targetId)
        {
            if (!IsADesiredTargetEntity(entityType))
            {
                return;
            }
            
            _targetsInsideVisionArea[entityType].Add(targetId);
            
            CombatManager.Instance.AddPreSightedTargetToCombatArea(_areaNumber, entityType, targetId);
        }

        private void RemoveTargetInsideVisionArea(EntityType entityType, uint targetId)
        {
            if (!IsADesiredTargetEntity(entityType))
            {
                return;
            }
            
            _targetsInsideVisionArea[entityType].Remove(targetId);
            
            CombatManager.Instance.RemovePreSightedTargetToCombatArea(_areaNumber, targetId);
        }

        protected abstract void RemoveATargetIfWasLost(uint targetIdToCheck);

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
    }
}