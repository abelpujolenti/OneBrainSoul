using ECS.Components.AI.Combat.Abilities;
using ECS.Entities.AI;
using UnityEngine;

namespace AI.Combat.AbilityAoEColliders
{
    public class RectangleAbilityAoECollider : AbilityAoECollider<RectangleAbilityComponent>
    {
        [SerializeField] private BoxCollider _boxCollider;

        private Vector3 _direction;

        protected override void OnEnable()
        {
            _stopwatch.Start();
            
            MoveToPosition(_relativePosition);
            Rotate();

            _actionAttaching();
        }

        protected override void OnDisable()
        {
            //_stopwatch.Reset();
        }

        public override void SetAbilitySpecs(RectangleAbilityComponent rectangleAbilityComponent)
        {
            base.SetAbilitySpecs(rectangleAbilityComponent);

            _direction = rectangleAbilityComponent.GetDirection().normalized;

            float height = rectangleAbilityComponent.GetHeight();
            float width = rectangleAbilityComponent.GetWidth();
            float length = rectangleAbilityComponent.GetLength();
            
            _boxCollider.size = new Vector3(width, height, length);

            /*Vector3 center = new Vector3
            {
                x = Convert.ToInt16(!_abilityComponent.IsRelativePositionXCenterOfColliderX()) * (width / 2),
                y = Convert.ToInt16(!_abilityComponent.IsRelativePositionYCenterOfColliderY()) * (height / 2),
                z = Convert.ToInt16(!_abilityComponent.IsRelativePositionZCenterOfColliderZ()) * (length / 2)
            };*/

            _boxCollider.center = Vector3.zero;
        }

        public override void SetAbilityTargets(int targetsLayerMask)
        {
            _boxCollider.includeLayers = targetsLayerMask;
            _boxCollider.excludeLayers = ~targetsLayerMask;
        }

        private void Rotate()
        {
            transform.rotation = _parentRotation * 
                    Quaternion.LookRotation(_direction, Vector3.up);
        }

        private void OnTriggerEnter(Collider other)
        {
            _agentsInside.Add(other.GetComponent<AgentEntity>());
        }

        private void OnTriggerExit(Collider other)
        {
            _agentsInside.Remove(other.GetComponent<AgentEntity>());
        }
    }
}