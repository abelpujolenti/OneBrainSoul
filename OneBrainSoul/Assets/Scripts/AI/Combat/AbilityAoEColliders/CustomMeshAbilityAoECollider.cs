using ECS.Components.AI.Combat.Abilities;
using UnityEngine;

namespace AI.Combat.AbilityAoEColliders
{
    public class CustomMeshAbilityAoECollider : AbilityAoECollider<CustomMeshAbilityComponent>
    {
        [SerializeField] private MeshCollider _meshCollider;

        private AnimationCurve _rescaleCurve;

        public override void SetAbilitySpecs(Transform parentTransform, CustomMeshAbilityComponent customMeshAbilityComponent)
        {
            base.SetAbilitySpecs(parentTransform, customMeshAbilityComponent);

            if (!customMeshAbilityComponent.GetAoE().doesScaleChangeOverTheTime)
            {
                return;
            }
            
            _rescaleCurve = customMeshAbilityComponent.GetAoE().scaleChangeOverTime;
            _actionResizing = time =>
            {
                float scale = ReturnSizeOverTime(time, _rescaleCurve);
                _meshCollider.gameObject.transform.localScale = new Vector3(scale, scale, scale);
            };
        }

        public override void SetAbilityTargets(int targetsLayerMask)
        {
            _meshCollider.includeLayers = targetsLayerMask;
            _meshCollider.excludeLayers = ~targetsLayerMask;
        }
    }
}