using System;
using System.Collections.Generic;
using ECS.Entities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;

        public static GameManager Instance => _instance;

        [Min(0f)] 
        [SerializeField] private float _playerReceiveDamageCooldown;
        
        [FormerlySerializedAs("_enemiesReceiveDamageCooldown")]
        [Min(0f)] 
        [SerializeField] private float _enemyReceiveDamageCooldown;
        
        [Min(0f)]
        [SerializeField] private float _timeBetweenDamageTicks;
        
        [Min(0f)]
        [SerializeField] private float _timeBetweenHealTicks;

        private int GROUND_LAYER = 6;
        private int INTERACTABLE_LAYER = 7;
        private int ENEMY_ATTACK_ZONE_LAYER = 23;

        private Dictionary<EntityType, int> _entitiesLayer = new Dictionary<EntityType, int>
        {
            { EntityType.PLAYER , (int)Math.Pow(2, 8)},
            { EntityType.TRIFACE , (int)Math.Pow(2, 9)},
            { EntityType.LONG_ARMS , (int)Math.Pow(2, 10)},
            { EntityType.LONG_ARMS_BASE , (int)Math.Pow(2, 11)},
            { EntityType.HEALER , (int)Math.Pow(2, 12)},
        };

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                
                DontDestroyOnLoad(gameObject);
                
                return;
            }
            
            Destroy(gameObject);
        }

        public float GetPlayerReceiveDamageCooldown()
        {
            return _playerReceiveDamageCooldown;
        }

        public float GetEnemyReceiveDamageCooldown()
        {
            return _enemyReceiveDamageCooldown;
        }

        public float GetTimeBetweenDamageTicks()
        {
            return _timeBetweenDamageTicks;
        }

        public float GetTimeBetweenHealTicks()
        {
            return _timeBetweenHealTicks;
        }

        public int GetGroundLayer()
        {
            return (int)Math.Pow(2, GROUND_LAYER);
        }

        public int GetInteractableLayer()
        {
            return (int)Math.Pow(2, INTERACTABLE_LAYER);
        }

        public int GetEnemyLayer()
        {
            return _entitiesLayer[EntityType.TRIFACE] + 
                   _entitiesLayer[EntityType.LONG_ARMS] +
                   _entitiesLayer[EntityType.LONG_ARMS_BASE] +
                   _entitiesLayer[EntityType.HEALER];
        }

        public int GetEnemyAttackZoneLayer()
        {
            return (int)Math.Pow(2, ENEMY_ATTACK_ZONE_LAYER);
        }

        public int GetRaycastLayers()
        {
            return GetInteractableLayer() + GetGroundLayer() + _entitiesLayer[EntityType.PLAYER] + GetEnemyLayer() + 1;
        }
        
        public int GetRaycastLayersWithoutAlly()
        {
            return GetInteractableLayer() + GetGroundLayer() + GetEnemyLayer() + 1;
        }

        public int GetEntityTypeLayer(EntityType entityType)
        {
            return _entitiesLayer[entityType];
        }

        public int GetDifferentEnemiesLayerFromMyType(EntityType entityType)
        {
            int layer = GetEnemyLayer();

            layer -= _entitiesLayer[entityType];

            return layer;
        }
    }
}