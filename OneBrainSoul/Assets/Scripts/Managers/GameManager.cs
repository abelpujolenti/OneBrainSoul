using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;

        public static GameManager Instance => _instance;

        private const int MAIN_MENU_SCENE_INDEX = 1;

        [Min(0f)] 
        [SerializeField] private float _playerReceiveDamageCooldown;
        
        [Min(0f)] 
        [SerializeField] private float _enemyReceiveDamageCooldown;
        
        [Min(0f)]
        [SerializeField] private float _timeBetweenDamageTicks;
        
        [Min(0f)]
        [SerializeField] private float _timeBetweenHealTicks;

        [SerializeField] private uint _healPerDeath;

        private int GROUND_LAYER = 6;
        private int INTERACTABLE_LAYER = 7;
        private int PLAYER_LAYER = 8;
        private int ENEMY_LAYER = 9;
        private int ENEMY_ATTACK_ZONE_LAYER = 10;

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

        public uint GetHealPerDeath()
        {
            return _healPerDeath;
        }

        public int GetGroundLayer()
        {
            return (int)Math.Pow(2, GROUND_LAYER);
        }

        public int GetInteractableLayer()
        {
            return (int)Math.Pow(2, INTERACTABLE_LAYER);
        }

        public int GetPlayerLayer()
        {
            return (int)Math.Pow(2, PLAYER_LAYER);
        }

        public int GetEnemyLayer()
        {
            return (int)Math.Pow(2, ENEMY_LAYER);
        }

        public int GetEnemyAttackZoneLayer()
        {
            return (int)Math.Pow(2, ENEMY_ATTACK_ZONE_LAYER);
        }

        public int GetRaycastLayers()
        {
            return GetInteractableLayer() + GetGroundLayer() + GetPlayerLayer() + GetEnemyLayer() + 1;
        }
        
        public int GetRaycastLayersWithoutAlly()
        {
            return GetInteractableLayer() + GetGroundLayer() + GetEnemyLayer() + 1;
        }

        public void LoadSceneIndex(int index)
        {
            SceneManager.LoadScene(index);
        }

        public void GoToMainMenu()
        {
            SceneManager.LoadScene(MAIN_MENU_SCENE_INDEX);
        }

        public void ExitGame()
        {
            Application.Quit();
        }
    }
}