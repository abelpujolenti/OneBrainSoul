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
            RectangularAbilityComponent sphericalAbilityComponent)
        {
            base.SetAbilitySpecs(parentTransform, basicAbilityComponent, sphericalAbilityComponent);
            
            _boxCollider.size = new Vector3(
                sphericalAbilityComponent.GetWidth(), 
                sphericalAbilityComponent.GetHeight(), 
                sphericalAbilityComponent.GetLength());

            /*Vector3 center = new Vector3
            {
                x = Convert.ToInt16(!_abilityComponent.IsRelativePositionXCenterOfColliderX()) * (width / 2),
                y = Convert.ToInt16(!_abilityComponent.IsRelativePositionYCenterOfColliderY()) * (height / 2),
                z = Convert.ToInt16(!_abilityComponent.IsRelativePositionZCenterOfColliderZ()) * (length / 2)
            };*/

            _boxCollider.center = Vector3.zero;
            
            if (sphericalAbilityComponent.GetAoE().doesHeightChangeOverTheTime)
            {
                _heightResizeCurve = sphericalAbilityComponent.GetAoE().heightChangeOverTime;
                _actionResizing = time =>
                {
                    Vector3 newSize = _boxCollider.size;
                    newSize.y = ReturnSizeOverTime(time, _heightResizeCurve);
                    _boxCollider.size = newSize;
                };
            }

            if (sphericalAbilityComponent.GetAoE().doesWidthChangeOverTheTime)
            {
                _widthResizeCurve = sphericalAbilityComponent.GetAoE().widthChangeOverTime;

                if (!sphericalAbilityComponent.GetAoE().doesHeightChangeOverTheTime)
                {
                    _actionResizing = time =>
                    {
                        Vector3 newSize = _boxCollider.size;
                        newSize.x = ReturnSizeOverTime(time, _widthResizeCurve);
                        _boxCollider.size = newSize;
                    };
                }
                else
                {
                    _actionResizing += time =>
                    {
                        Vector3 newSize = _boxCollider.size;
                        newSize.x = ReturnSizeOverTime(time, _widthResizeCurve);
                        _boxCollider.size = newSize;
                    };
                }
            }

            if (!sphericalAbilityComponent.GetAoE().doesLengthChangeOverTheTime)
            {
                return;
            }
            
            _lengthResizeCurve = sphericalAbilityComponent.GetAoE().lengthChangeOverTime;

            if (_actionResizing.GetInvocationList().Length == 0)
            {
                _actionResizing = time =>
                {
                    Vector3 newSize = _boxCollider.size;
                    newSize.z = ReturnSizeOverTime(time, _lengthResizeCurve);
                    _boxCollider.size = newSize;
                };
                return;
            }
            
            _actionResizing += time =>
            {
                Vector3 newSize = _boxCollider.size;
                newSize.z = ReturnSizeOverTime(time, _lengthResizeCurve);
                _boxCollider.size = newSize;
            };
        }
    }
}