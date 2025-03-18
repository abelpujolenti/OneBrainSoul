using AI.Combat.ScriptableObjects;

namespace ECS.Components.AI.Combat.Abilities
{
    public class SphericalAbilityComponent : AreaAbilityComponent
    {
        private float _radius;

        public SphericalAbilityComponent(AreaAbilityProperties areaBasicAbilityProperties) : base(areaBasicAbilityProperties)
        {
            _radius = areaBasicAbilityProperties.abilityAoE.radius;
        }

        public float GetRadius()
        {
            return _radius;
        }
    }
}