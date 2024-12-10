using System;
using AI.Combat.AttackColliders;
using ECS.Components.AI.Combat;
using ECS.Entities.AI.Combat;
using UnityEngine;

namespace AI.Combat.Ally
{
    public class AIAllyRectangleAttackCollider : AIAllyAttackCollider
    {
        [SerializeField] private BoxCollider _boxCollider;
        
        private AllyRectangleAttackComponent _rectangleAttackComponent; 

        protected override void OnEnable()
        {
            if (_rectangleAttackComponent == null)
            {
                return;
            }
            
            MoveToPosition(_rectangleAttackComponent.GetRelativePosition());
            Rotate();

            if (_rectangleAttackComponent.IsAttachedToAttacker())
            {
                return;
            }

            transform.parent = null;
        }

        private void Rotate()
        {
            transform.rotation = 
                _parentRotation * Quaternion.LookRotation(_rectangleAttackComponent.GetDirection().normalized, Vector3.up);
        }

        public override void SetAttackTargets(int targetsLayerMask)
        {
            _boxCollider.includeLayers = targetsLayerMask;
            _boxCollider.excludeLayers = ~targetsLayerMask;
        }

        public void SetRectangleAttackComponent(AllyRectangleAttackComponent rectangleAttackComponent)
        {
            _allyID = rectangleAttackComponent.GetAllyID();
            
            _rectangleAttackComponent = rectangleAttackComponent;

            float height = _rectangleAttackComponent.GetHeight();
            float width = _rectangleAttackComponent.GetWidth();
            float length = _rectangleAttackComponent.GetLength();
            
            _boxCollider.size = new Vector3(width, height, length);

            Vector3 center = new Vector3
            {
                x = Convert.ToInt16(!_rectangleAttackComponent.IsRelativePositionXCenterOfColliderX()) * (width / 2),
                y = Convert.ToInt16(!_rectangleAttackComponent.IsRelativePositionYCenterOfColliderY()) * (height / 2),
                z = Convert.ToInt16(!_rectangleAttackComponent.IsRelativePositionZCenterOfColliderZ()) * (length / 2)
            };

            _boxCollider.center = center;
        }

        public override void StartInflictingDamage()
        {
            foreach (AIEnemy enemy in _combatAgentsTriggering)
            {
                InflictDamageToEnemy(enemy);
            }
        }

        private void InflictDamageToEnemy(AIEnemy enemy)
        {
            enemy.OnReceiveDamage(new AllyDamageComponent(_rectangleAttackComponent.GetDamage(),
                _rectangleAttackComponent.GetStressDamage()));
        }

        private void OnTriggerEnter(Collider other)
        {
            AIEnemy targetEnemy = other.GetComponent<AIEnemy>();

            if (_combatAgentsTriggering.Contains(targetEnemy))
            {
                return;
            }
            
            _combatAgentsTriggering.Add(targetEnemy);
        }
    }
}