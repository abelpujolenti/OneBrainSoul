using ECS.Components.AI.Combat.Abilities;
using UnityEngine;

namespace AI.Combat.AbilityAoEColliders
{
    public class CustomMeshAbilityAoECollider : AbilityAoECollider<CustomMeshAbilityComponent>
    {
        [SerializeField] private MeshCollider _meshCollider;

        private GameObject _gameObject;

        private AnimationCurve _rescaleCurve;
        private AnimationCurve _XRescaleCurve;
        private AnimationCurve _YRescaleCurve;
        private AnimationCurve _ZRescaleCurve;

        public override void SetAbilitySpecs(Transform parentTransform, BasicAbilityComponent basicAbilityComponent, 
            CustomMeshAbilityComponent customMeshAbilityComponent)
        {
            base.SetAbilitySpecs(parentTransform, basicAbilityComponent, customMeshAbilityComponent);

            if (customMeshAbilityComponent.GetAoE().doesScaleChangeOverTheTime)
            {
                _rescaleCurve = customMeshAbilityComponent.GetAoE().scaleChangeOverTime;
                _actionResizing = time =>
                {
                    float scale = ReturnSizeOverTime(time, _rescaleCurve);
                    _meshCollider.gameObject.transform.localScale = new Vector3(scale, scale, scale);
                };
                return;
            }

            if (customMeshAbilityComponent.GetAoE().doesXScaleChangeOverTheTime)
            {
                _XRescaleCurve = customMeshAbilityComponent.GetAoE().XScaleChangeOverTime;
                _actionResizing = time =>
                {
                    Vector3 newSize = _gameObject.transform.localScale;
                    newSize.x = ReturnSizeOverTime(time, _XRescaleCurve);
                    _gameObject.transform.localScale = newSize;

                    Vector3 scale = _childWithParticleSystem.transform.localScale;
                    scale.x = newSize.x / _XRescaleCurve.keys[0].value;
                    _childWithParticleSystem.transform.localScale = scale;
                };
            }

            if (customMeshAbilityComponent.GetAoE().doesYScaleChangeOverTheTime)
            {
                _YRescaleCurve = customMeshAbilityComponent.GetAoE().YScaleChangeOverTime;
                if (!customMeshAbilityComponent.GetAoE().doesXScaleChangeOverTheTime)
                {
                    _actionResizing = time =>
                    {
                        Vector3 newSize = _gameObject.transform.localScale;
                        newSize.y = ReturnSizeOverTime(time, _YRescaleCurve);
                        _gameObject.transform.localScale = newSize;

                        Vector3 scale = _childWithParticleSystem.transform.localScale;
                        scale.y = newSize.y / _YRescaleCurve.keys[0].value;
                        _childWithParticleSystem.transform.localScale = scale;
                    };    
                }
                else
                {
                    _actionResizing += time =>
                    {
                        Vector3 newSize = _gameObject.transform.localScale;
                        newSize.y = ReturnSizeOverTime(time, _YRescaleCurve);
                        _gameObject.transform.localScale = newSize;

                        Vector3 scale = _childWithParticleSystem.transform.localScale;
                        scale.y = newSize.y / _YRescaleCurve.keys[0].value;
                        _childWithParticleSystem.transform.localScale = scale;
                    }; 
                }
                
            }

            if (!customMeshAbilityComponent.GetAoE().doesZScaleChangeOverTheTime)
            {
                return;
            }
            
            _ZRescaleCurve = customMeshAbilityComponent.GetAoE().ZScaleChangeOverTime;

            if (_actionResizing.GetInvocationList().Length == 0)
            {
                _actionResizing = time =>
                {
                    Vector3 newSize = _gameObject.transform.localScale;
                    newSize.z = ReturnSizeOverTime(time, _ZRescaleCurve);
                    _gameObject.transform.localScale = newSize;

                    Vector3 scale = _childWithParticleSystem.transform.localScale;
                    scale.z = newSize.z / _ZRescaleCurve.keys[0].value;
                    _childWithParticleSystem.transform.localScale = scale;
                };
                return;
            }
            
            _actionResizing += time =>
            {
                Vector3 newSize = _gameObject.transform.localScale;
                newSize.z = ReturnSizeOverTime(time, _ZRescaleCurve);
                _gameObject.transform.localScale = newSize;

                Vector3 scale = _childWithParticleSystem.transform.localScale;
                scale.z = newSize.z / _ZRescaleCurve.keys[0].value;
                _childWithParticleSystem.transform.localScale = scale;
            };
        }
    }
}