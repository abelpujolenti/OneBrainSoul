using AI.Combat.AbilityColliders;
using ECS.Components.AI.Combat;
using UnityEngine;

namespace AI.Combat.Enemy
{
    public class AIEnemyConeAbilityCollider : AIEnemyAbilityCollider
    {
        private ConeAttackComponent _coneAttackComponent;

        private SphereCollider _sphereCollider;

        protected override void OnEnable()
        {
            if (_coneAttackComponent == null)
            {
                return;
            }
            
            _stopwatch.Reset();
            _stopwatch.Start();
            
            MoveToPosition(_coneAttackComponent.GetRelativePosition());
            Rotate();

            if (!_coneAttackComponent.IsAttachedToAttacker())
            {
                return;
            }

            transform.parent = null;
        }

        private void Rotate()
        {
            transform.rotation = 
                _parentRotation * Quaternion.LookRotation(_coneAttackComponent.GetDirection().normalized, Vector3.up);
        }

        public override void SetAbilityTargets(int targetsLayerMask)
        {
            _sphereCollider.includeLayers = targetsLayerMask;
            _sphereCollider.excludeLayers = ~targetsLayerMask;
        }

        public void SetConeAbilityComponent(ConeAttackComponent coneAttackComponent)
        {
            _sphereCollider = gameObject.AddComponent<SphereCollider>();
            _sphereCollider.isTrigger = true;
            
            _coneAttackComponent = coneAttackComponent;
            
            Rotate();
        }
    }
}