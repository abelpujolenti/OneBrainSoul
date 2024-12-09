using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;

        public static GameManager Instance => _instance;

        private int GROUND_LAYER = 6;
        private int ALLY_LAYER = 8;
        private int ENEMY_LAYER = 9;
        private int ALLY_ATTACK_ZONE = 10;
        private int ENEMY_ATTACK_ZONE = 11;

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

        public int GetAllyLayer()
        {
            return ALLY_LAYER;
        }

        public int GetEnemyLayer()
        {
            return ENEMY_LAYER;
        }

        public int GetGroundLayer()
        {
            return GROUND_LAYER;
        }

        public int GetAllyAttackZoneLayer()
        {
            return ALLY_ATTACK_ZONE;
        }

        public int GetEnemyAttackZoneLayer()
        {
            return ENEMY_ATTACK_ZONE;
        }
    }
}