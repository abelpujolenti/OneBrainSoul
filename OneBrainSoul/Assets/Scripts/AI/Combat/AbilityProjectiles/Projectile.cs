using System;
using Interfaces.AI.Combat;
using UnityEngine;

namespace AI.Combat.AbilityProjectiles
{
    public class Projectile: MonoBehaviour
    {
        [SerializeField] private Rigidbody _rigidbody;
        
        private float _speed;

        private Action _onFireAction = () => { };
        private Action _onCollideAction = () => { };
        
        private IAbilityCollider _abilityCollider;

        public void SetProjectileSpecs(float projectileSpeed, bool makesAParabola)
        {
            _speed = projectileSpeed;

            if (!makesAParabola)
            {
                return;
            }

            _onFireAction = () => _rigidbody.useGravity = true;
            _onCollideAction = () => _rigidbody.useGravity = false;
        }

        public void SetAbilityCollider(IAbilityCollider abilityColliderCollider)
        {
            _abilityCollider = abilityColliderCollider;
        }

        public void ResetProjectile(Transform parentTransform, Vector3 relativePosition)
        {
            _onCollideAction();
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            
            Transform ownTransform = transform;
            ownTransform.parent = parentTransform;
            ownTransform.localPosition = relativePosition;
            ownTransform.localRotation = Quaternion.identity;
        }

        public void FIREEEEEEEEEEEE(Vector3 forceVector)
        {
            _onFireAction();
            transform.rotation = Quaternion.LookRotation(forceVector.normalized);
            gameObject.SetActive(true);
            transform.parent = null;
            _rigidbody.AddForce(forceVector, ForceMode.Impulse);
        }

        private void BOOOOOOM()
        {
            _abilityCollider.Activate();
            gameObject.SetActive(false);
        }

        private void OnCollisionEnter(Collision other)
        {
            BOOOOOOM();
        }

        public float GetMass()
        {
            return _rigidbody.mass;
        }

        public float GetSpeed()
        {
            return _speed;
        }
    }
}