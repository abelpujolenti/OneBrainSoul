﻿using System;
using System.Collections;
using ECS.Entities;
using ECS.Entities.AI;
using Managers;
using Player.Camera;
using Player.Effects;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Player
{
    public class PlayerCharacter : AgentEntity
    {
        [SerializeField] private FirstPersonCamera _camera;
        [SerializeField] private PlayerCharacterController _playerCharacterController;
        private Hitstop _hitstop;

        [SerializeField] private uint _maxHealth;
        [SerializeField] private float _agentsPositionRadius;
        [SerializeField] private float _damageEffectDuration;

        [SerializeField] private float killHitstop = 0.15f;
        [SerializeField] private float onDamageHitstop = 0.15f;

        [SerializeField] private float ghostDuration = 60f;
        [SerializeField] private float ghostSpeed = 1.2f;
        private float _ghostTime = -1f;
        private Coroutine ghostTimerCoroutine;
        private bool _hasBody = true;
        [SerializeField] Transform corpsePrefab;
        private CombatRoom currCombatRoom;

        private uint _areasDetecting = 0;

        private uint _health;
        private float _height;
        private float _radius;

        [SerializeField] private uint _healthLostPerTick;
        [SerializeField] private float _timeBetweenTicks;
        private float _currentTimeBetweenTicks = 0;

        private void Start()
        {
            _health = _maxHealth;

            _receiveDamageCooldown = GameManager.Instance.GetPlayerReceiveDamageCooldown();

            CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
            _hitstop = GetComponent<Hitstop>();

            EventsManager.OnDefeatEnemy += DefeatAgent;

            _height = capsuleCollider.height;
            _radius = capsuleCollider.radius;
            
            Setup(_radius + _agentsPositionRadius, EntityType.PLAYER);
            
            CombatManager.Instance.AddPlayer(this);
        }

        private void Update()
        {
            _currentTimeBetweenTicks += Time.deltaTime;

            if (_currentTimeBetweenTicks < _timeBetweenTicks)
            {
                return;
            }

            _currentTimeBetweenTicks = 0;
            _health -= _healthLostPerTick;
        }

        private void OnDestroy()
        {
            EventsManager.OnDefeatEnemy -= DefeatAgent;
        }

        private void DefeatAgent()
        {
            _hitstop.Add(killHitstop);
            _hitstop.AddAftershock(killHitstop * 1.5f);
        }

        public void RechargeHookCharge()
        {
            _playerCharacterController.AddHookCharge();
        }

        public override void OnReceiveDamage(uint damageValue, Vector3 hitPosition, Vector3 sourcePosition)
        {
            if (_currentReceiveDamageCooldown > 0f || GetEntityType() == EntityType.GHOST)
            {
                return;
            }
            
            ShowDebugMessages("Player Damage : " + damageValue);

            _health = (uint)Mathf.Max(0f, _health - damageValue);

            if (_health == 0)
            {
                //Player Die
                SetEntityType(EntityType.GHOST);
                _playerCharacterController.SetMoveSpeedMultiplier(ghostSpeed);
                _hasBody = false;
                Vector3 deathPos = transform.position;
                _playerCharacterController.Respawn();
                Instantiate(corpsePrefab, deathPos + Vector3.up * 0.75f, Quaternion.identity);
                ghostTimerCoroutine = StartCoroutine(GhostTimerCoroutine());
                PostProcessingManager.Instance.GhostEffect(ghostDuration);
                _hitstop.Add(0.5f);
                if (currCombatRoom != null)
                {
                    currCombatRoom.ResetRoom();
                }
                return;
            }
            
            DamageEffect(hitPosition);
            
            PostProcessingManager.Instance.DamageEffect(_damageEffectDuration);
            _camera.ScreenShake(_damageEffectDuration * 0.65f, .9f);
            _hitstop.Add(onDamageHitstop);
            
            StartCoroutine(DecreaseDamageCooldown());
        }

        private IEnumerator GhostTimerCoroutine()
        {
            _ghostTime = 0f;
            while (_ghostTime < ghostDuration && !_hasBody)
            {
                yield return new WaitForFixedUpdate();
                _ghostTime += Time.fixedDeltaTime;
            }
            if (!_hasBody)
            {
                //TODO: not this garbage
                var spawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.InstanceID);
                foreach (EnemySpawner spawner in spawners)
                {
                    if (spawner.spawnOnStart && !spawner.HasLiveSpawns())
                    {
                        spawner.canSpawn = true;
                        spawner.Spawn();
                    }
                }
                Destroy(FindObjectOfType<Corpse>().gameObject);
                RecoverBody();
                AudioManager.instance.PlayOneShot(FMODEvents.instance.heal, transform.position);
                AudioManager.instance.PlayOneShot(FMODEvents.instance.wandAttack, transform.position);
            }
        }

        public void RecoverBody()
        {
            _hasBody = true;
            _playerCharacterController.SetMoveSpeedMultiplier(1f);
            _health = _maxHealth;
            _ghostTime = -1;
            SetEntityType(EntityType.PLAYER);
            PostProcessingManager.Instance.RecoverGhostEffect(.65f);
            AudioManager.instance.PlayOneShot(FMODEvents.instance.healed, transform.position);
            AudioManager.instance.PlayOneShot(FMODEvents.instance.wandAttack, transform.position);

        }

        public void EnterCombatRoom(CombatRoom c)
        {
            currCombatRoom = c;
        }

        public void DefeatCombatRoom()
        {
            currCombatRoom = null;
        }

        public override void OnReceiveDamageOverTime(uint damageValue, float duration, Vector3 sourcePosition)
        {
            ShowDebugMessages("Player Damage Over Time: " + damageValue + " - Duration: " + duration);

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

        public override void OnReceiveHeal(uint healValue, Vector3 sourcePosition)
        {
            _health = (uint)Mathf.Max(_maxHealth, _health + healValue);
            AudioManager.instance.PlayOneShot(FMODEvents.instance.healed, transform.position);
        }

        public override void OnReceiveHealOverTime(uint healValue, float duration, Vector3 sourcePosition)
        {
            ShowDebugMessages("Player Heal Over Time: " + healValue + " - Duration: " + duration);

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

        public override void OnReceiveSlow(uint slowID, uint slowPercent, Vector3 sourcePosition)
        {
            ShowDebugMessages("Player Slow: " + slowPercent);

            if (_slowSubscriptions.ContainsKey(slowID))
            {
                return;
            }
            
            _slowSubscriptions.Add(slowID, _slowEffects.Count);
            _slowEffects.Add(slowPercent);
        }

        public override void OnReleaseFromSlow(uint slowID)
        {
            ShowDebugMessages("Player Release From Slow");

            _slowEffects.RemoveAt(_slowSubscriptions[slowID]);
            _slowSubscriptions.Remove(slowID);
        }

        public override void OnReceiveSlowOverTime(uint slowID, uint slowPercent, float duration, Vector3 sourcePosition)
        {
            ShowDebugMessages("Player Slow Over Time: " + slowPercent + " - Duration: " + duration);

            if (_slowCoroutinesSubscriptions.ContainsKey(slowID))
            {
                StopCoroutine(_slowCoroutinesSubscriptions[slowID].Item2);
                _slowEffects.RemoveAt(_slowCoroutinesSubscriptions[slowID].Item1);
                
                _slowCoroutinesSubscriptions[slowID] = new Tuple<int, Coroutine>(_slowEffects.Count,
                    StartCoroutine(SlowOverTimeCoroutine(slowID, slowPercent, duration, sourcePosition)));
                _slowEffects.Add(slowPercent);
                return;
            }

            _slowCoroutinesSubscriptions.Add(slowID, new Tuple<int, Coroutine>(_slowEffects.Count,
                StartCoroutine(SlowOverTimeCoroutine(slowID, slowPercent, duration, sourcePosition))));
            _slowEffects.Add(slowPercent);
        }

        protected override IEnumerator SlowOverTimeCoroutine(uint slowID, uint slowPercent, float duration, Vector3 sourcePosition)
        {
            float timer = 0;

            while (timer < duration)
            {
                timer += Time.deltaTime;

                //TODO SLOW EFFECT
                
                yield return null;
            }

            _slowEffects.RemoveAt(_slowCoroutinesSubscriptions[slowID].Item1);
            _slowCoroutinesSubscriptions.Remove(slowID);
        }

        public override void OnReceiveDecreasingSlow(uint slowID, uint slowPercent, float duration, Vector3 sourcePosition)
        {
            ShowDebugMessages("Player Decreasing Slow: " + slowPercent + " - Duration: " + duration);

            if (_slowCoroutinesSubscriptions.ContainsKey(slowID))
            {
                StopCoroutine(_slowCoroutinesSubscriptions[slowID].Item2);
                _slowEffects.RemoveAt(_slowCoroutinesSubscriptions[slowID].Item1);
                
                _slowCoroutinesSubscriptions[slowID] = new Tuple<int, Coroutine>(_slowEffects.Count,
                    StartCoroutine(SlowOverTimeCoroutine(slowID, slowPercent, duration, sourcePosition)));
                _slowEffects.Add(slowPercent);
                return;
            }

            _slowCoroutinesSubscriptions.Add(slowID, new Tuple<int, Coroutine>(_slowEffects.Count,
                StartCoroutine(SlowOverTimeCoroutine(slowID, slowPercent, duration, sourcePosition))));
            _slowEffects.Add(slowPercent);
        }

        protected override IEnumerator DecreasingSlowCoroutine(uint slowID, uint slowPercent, float duration, int slot, Vector3 sourcePosition)
        {
            float timer = 0;

            while (timer < duration)
            {
                timer += Time.deltaTime;

                _slowEffects[slot] = Mathf.Lerp(slowPercent, 0, timer);
                
                //TODO DECREASE SLOW EFFECT
                
                yield return null;
            }

            _slowEffects.RemoveAt(_slowCoroutinesSubscriptions[slowID].Item1);
            _slowCoroutinesSubscriptions.Remove(slowID);
        }

        public override void OnReceivePushFromCenter(Vector3 centerPosition, Vector3 forceDirection, float forceStrength, 
            Vector3 sourcePosition, ForceMode forceMode)
        {
            if (!_playerCharacterController.CanBeDisplaced())
            {
                return;
            }
            
            base.OnReceivePushFromCenter(centerPosition, forceDirection, forceStrength, sourcePosition, forceMode);
            _playerCharacterController.ChangeMovementHandlerToAirborne();
        }

        public override void OnReceivePushInADirection(Vector3 colliderForwardVector, Vector3 forceDirection, 
            float forceStrength, Vector3 sourcePosition, ForceMode forceMode)
        {
            if (!_playerCharacterController.CanBeDisplaced())
            {
                return;
            }

            base.OnReceivePushInADirection(colliderForwardVector, forceDirection, forceStrength, sourcePosition,
                forceMode);
            
            _playerCharacterController.ChangeMovementHandlerToAirborne();
        }

        public void WhenDetected()
        {
            _playerCharacterController.SetInCombat(true);
            _areasDetecting++;
        }

        public void WhenDetectionLost()
        {
            _areasDetecting--;

            if (_areasDetecting != 0)
            {
                return;
            }

            _playerCharacterController.SetInCombat(false);
        }

        public override float GetHeight()
        {
            return _height;
        }

        public override float GetRadius()
        {
            return _radius;
        }
        public uint GetHealth()
        {
            return _health;
        }

        public uint GetMaxHealth()
        {
            return _maxHealth;
        }

        public float GetGhostTimeNormalized()
        {
            return _ghostTime / ghostDuration;
        }
    }
}