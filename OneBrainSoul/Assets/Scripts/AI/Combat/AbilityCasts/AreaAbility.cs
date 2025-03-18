using AI.Combat.AbilityAoEColliders;
using AI.Combat.AbilitySpecs;
using ECS.Components.AI.Combat.Abilities;
using Interfaces.AI.Combat;

namespace AI.Combat.AbilityCasts
{
    public class AreaAbility<TAreaAbilityComponent, TAbilityCollider> : IAreaAbility
        where TAbilityCollider : AbilityAoECollider<TAreaAbilityComponent>
        where TAreaAbilityComponent : AreaAbilityComponent
    {
        private BasicAbilityComponent _basicAbilityComponent;
        private TAbilityCollider _abilityCollider;
        
        private uint _targetId;

        public AreaAbility(BasicAbilityComponent basicAbilityComponent, TAbilityCollider abilityCollider)
        {
            _basicAbilityComponent = basicAbilityComponent;
            _abilityCollider = abilityCollider;
        }

        public void Activate()
        {
            _abilityCollider.Activate();
        }

        public AbilityCast GetCast()
        {
            return _basicAbilityComponent.GetCast();
        }

        public void SetTargetId(uint targetId)
        {
            _targetId = targetId;
        }

        public uint GetTargetId()
        {
            return _targetId;
        }
    }
}