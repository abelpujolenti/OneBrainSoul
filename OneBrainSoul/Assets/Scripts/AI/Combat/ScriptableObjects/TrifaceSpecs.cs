using UnityEditor;
using UnityEngine;

namespace AI.Combat.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Triface Properties", menuName = "ScriptableObjects/AI/Combat/Entity/Triface Properties", order = 1)]
    public class TrifaceSpecs : AIEnemySpecs
    {
        [SerializeField] private AgentAbility _slamAbility;
        
        public AgentAbility SlamAbility => _slamAbility;

        private void OnEnable()
        {
            if (_slamAbility != null)
            {
                return;
            }

            _slamAbility = CreateInstance<AgentAbility>();
            AssetDatabase.AddObjectToAsset(_slamAbility, this);
            AssetDatabase.SaveAssets();
        }
    }
}