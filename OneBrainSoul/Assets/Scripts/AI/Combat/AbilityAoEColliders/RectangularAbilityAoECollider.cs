using ECS.Components.AI.Combat.Abilities;
using UnityEngine;

namespace AI.Combat.AbilityAoEColliders
{
    public class RectangularAbilityAoECollider : AbilityAoECollider<RectangularAbilityComponent>
    {
        [SerializeField] private BoxCollider _boxCollider;

        private AnimationCurve _widthResizeCurve;
        private AnimationCurve _heightResizeCurve;
        private AnimationCurve _lengthResizeCurve;

        public override void SetAbilitySpecs(Transform parentTransform, BasicAbilityComponent basicAbilityComponent, 
            RectangularAbilityComponent rectangularAbilityComponent)
        {
            base.SetAbilitySpecs(parentTransform, basicAbilityComponent, rectangularAbilityComponent);
            
            _boxCollider.size = new Vector3(
                rectangularAbilityComponent.GetWidth(), 
                rectangularAbilityComponent.GetHeight(), 
                rectangularAbilityComponent.GetLength());

            /*Vector3 center = new Vector3
            {
                x = Convert.ToInt16(!_abilityComponent.IsRelativePositionXCenterOfColliderX()) * (width / 2),
                y = Convert.ToInt16(!_abilityComponent.IsRelativePositionYCenterOfColliderY()) * (height / 2),
                z = Convert.ToInt16(!_abilityComponent.IsRelativePositionZCenterOfColliderZ()) * (length / 2)
            };*/

            _boxCollider.center = Vector3.zero;
            
            if (rectangularAbilityComponent.GetAoE().doesHeightChangeOverTheTime)
            {
                _heightResizeCurve = rectangularAbilityComponent.GetAoE().heightChangeOverTime;
                _actionResizing = time =>
                {
                    Vector3 newSize = _boxCollider.size;
                    newSize.y = ReturnSizeOverTime(time, _heightResizeCurve);
                    _boxCollider.size = newSize;

                    Vector3 scale = _childWithParticleSystem.transform.localScale;
                    scale.y = newSize.y / _heightResizeCurve.keys[0].value;
                    _childWithParticleSystem.transform.localScale = scale;
                };
            }

            if (rectangularAbilityComponent.GetAoE().doesWidthChangeOverTheTime)
            {
                _widthResizeCurve = rectangularAbilityComponent.GetAoE().widthChangeOverTime;

                if (!rectangularAbilityComponent.GetAoE().doesHeightChangeOverTheTime)
                {
                    _actionResizing = time =>
                    {
                        Vector3 newSize = _boxCollider.size;
                        newSize.x = ReturnSizeOverTime(time, _widthResizeCurve);
                        _boxCollider.size = newSize;

                        Vector3 scale = _childWithParticleSystem.transform.localScale;
                        scale.x = newSize.x / _widthResizeCurve.keys[0].value;
                        _childWithParticleSystem.transform.localScale = scale;
                    };
                }
                else
                {
                    _actionResizing += time =>
                    {
                        Vector3 newSize = _boxCollider.size;
                        newSize.x = ReturnSizeOverTime(time, _widthResizeCurve);
                        _boxCollider.size = newSize;

                        Vector3 scale = _childWithParticleSystem.transform.localScale;
                        scale.x = newSize.x / _widthResizeCurve.keys[0].value;
                        _childWithParticleSystem.transform.localScale = scale;
                    };
                }
            }

            if (!rectangularAbilityComponent.GetAoE().doesLengthChangeOverTheTime)
            {
                return;
            }
            
            _lengthResizeCurve = rectangularAbilityComponent.GetAoE().lengthChangeOverTime;

            if (_actionResizing.GetInvocationList().Length == 0)
            {
                _actionResizing = time =>
                {
                    Vector3 newSize = _boxCollider.size;
                    newSize.z = ReturnSizeOverTime(time, _lengthResizeCurve);
                    _boxCollider.size = newSize;

                    Vector3 scale = _childWithParticleSystem.transform.localScale;
                    scale.z = newSize.z / _lengthResizeCurve.keys[0].value;
                    _childWithParticleSystem.transform.localScale = scale;
                };
                return;
            }
            
            _actionResizing += time =>
            {
                Vector3 newSize = _boxCollider.size;
                newSize.z = ReturnSizeOverTime(time, _lengthResizeCurve);
                _boxCollider.size = newSize;

                Vector3 scale = _childWithParticleSystem.transform.localScale;
                scale.z = newSize.z / _lengthResizeCurve.keys[0].value;
                _childWithParticleSystem.transform.localScale = scale;
            };
        }
    }
}