using System;
using System.Collections;
using ECS.Entities;
using ECS.Entities.AI;
using Managers;
using Player.Camera;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Player
{
    public class PlayerCharacter : AgentEntity
    {
        [SerializeField] private FirstPersonCamera _camera;

        [SerializeField] private uint _maxHealth;
        [SerializeField] private float _agentsPositionRadius;
        [SerializeField] private float _damageEffectDuration;

        private uint _health;
        private float _height;
        private float _radius;

        private void Start()
        {
            _health = _maxHealth;

            _receiveDamageCooldown = GameManager.Instance.GetPlayerReceiveDamageCooldown();

            CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();

            _height = capsuleCollider.height;
            _radius = capsuleCollider.radius;
            
            Setup(_radius + _agentsPositionRadius, EntityType.PLAYER);
            
            CombatManager.Instance.AddPlayer(this);
        }

        public override void OnReceiveDamage(uint damageValue, Vector3 hitPosition)
        {
            if (_currentReceiveDamageCooldown > 0f)
            {
                return;
            }
            
            ShowDebugMessages("Player Damage : " + damageValue);

            _health = (uint)Mathf.Max(0f, _health - damageValue);

            if (_health == 0)
            {
                SceneManager.LoadScene("ControllerTest", LoadSceneMode.Single);
                //TODO PLAYER DIE
                return;
            }
            
            DamageEffect(hitPosition);
            
            PostProcessingManager.Instance.DamageEffect(_damageEffectDuration);
            _camera.ScreenShake(_damageEffectDuration * .25f, 1f);
            
            StartCoroutine(DecreaseDamageCooldown());
        }

        public override void OnReceiveDamageOverTime(uint damageValue, float duration)
        {
            ShowDebugMessages("Player Damage Over Time: " + damageValue + " - Duration: " + duration);

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

        public override void OnReceiveHeal(uint healValue)
        {
            _health = (uint)Mathf.Max(_maxHealth, _health + healValue);
        }

        public override void OnReceiveHealOverTime(uint healValue, float duration)
        {
            ShowDebugMessages("Player Heal Over Time: " + healValue + " - Duration: " + duration);

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

        public override void OnReceiveSlow(uint slowID, uint slowPercent)
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

        public override void OnReceiveSlowOverTime(uint slowID, uint slowPercent, float duration)
        {
            ShowDebugMessages("Player Slow Over Time: " + slowPercent + " - Duration: " + duration);

            if (_slowCoroutinesSubscriptions.ContainsKey(slowID))
            {
                StopCoroutine(_slowCoroutinesSubscriptions[slowID].Item2);
                _slowEffects.RemoveAt(_slowCoroutinesSubscriptions[slowID].Item1);
                
                _slowCoroutinesSubscriptions[slowID] = new Tuple<int, Coroutine>(_slowEffects.Count,
                    StartCoroutine(SlowOverTimeCoroutine(slowID, slowPercent, duration)));
                _slowEffects.Add(slowPercent);
                return;
            }

            _slowCoroutinesSubscriptions.Add(slowID, new Tuple<int, Coroutine>(_slowEffects.Count,
                StartCoroutine(SlowOverTimeCoroutine(slowID, slowPercent, duration))));
            _slowEffects.Add(slowPercent);
        }

        protected override IEnumerator SlowOverTimeCoroutine(uint slowID, uint slowPercent, float duration)
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

        public override void OnReceiveDecreasingSlow(uint slowID, uint slowPercent, float duration)
        {
            ShowDebugMessages("Player Decreasing Slow: " + slowPercent + " - Duration: " + duration);

            if (_slowCoroutinesSubscriptions.ContainsKey(slowID))
            {
                StopCoroutine(_slowCoroutinesSubscriptions[slowID].Item2);
                _slowEffects.RemoveAt(_slowCoroutinesSubscriptions[slowID].Item1);
                
                _slowCoroutinesSubscriptions[slowID] = new Tuple<int, Coroutine>(_slowEffects.Count,
                    StartCoroutine(SlowOverTimeCoroutine(slowID, slowPercent, duration)));
                _slowEffects.Add(slowPercent);
                return;
            }

            _slowCoroutinesSubscriptions.Add(slowID, new Tuple<int, Coroutine>(_slowEffects.Count,
                StartCoroutine(SlowOverTimeCoroutine(slowID, slowPercent, duration))));
            _slowEffects.Add(slowPercent);
        }

        protected override IEnumerator DecreasingSlowCoroutine(uint slowID, uint slowPercent, float duration, int slot)
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

        public override float GetHeight()
        {
            return _height;
        }

        public override float GetRadius()
        {
            return _radius;
        }
    }
}