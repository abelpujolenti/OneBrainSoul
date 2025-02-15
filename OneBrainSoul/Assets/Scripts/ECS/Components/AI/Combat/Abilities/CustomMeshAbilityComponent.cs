using AI.Combat.ScriptableObjects;

namespace ECS.Components.AI.Combat.Abilities
{
    public class CustomMeshAbilityComponent : AreaAbilityComponent
    {
        private float _scale;

        public CustomMeshAbilityComponent(AreaAbilityProperties areaBasicAbilityProperties) : base(areaBasicAbilityProperties)
        {
            _scale = areaBasicAbilityProperties.abilityAoE.scale;
        }

        public float GetScale()
        {
            return _scale;
        }
    }
}