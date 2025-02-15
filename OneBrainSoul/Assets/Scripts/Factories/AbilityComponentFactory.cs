using AI.Combat.ScriptableObjects;
using ECS.Components.AI.Combat.Abilities;

namespace Factories
{
    public static class AbilityComponentFactory
    {
        public delegate TAbilityComponent AbilityFactoryDelegate<out TAbilityComponent>(BasicAbilityProperties agentAbilityProperties)
            where TAbilityComponent : BasicAbilityComponent;

        public static TAbilityComponent InstantiateAbilityComponent<TAbilityComponent>(BasicAbilityProperties agentAbilityProperties,
            AbilityFactoryDelegate<TAbilityComponent> factoryDelegate)
            where TAbilityComponent : BasicAbilityComponent
        {
            return factoryDelegate(agentAbilityProperties);
        }
    }
}