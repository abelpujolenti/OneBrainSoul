using System;
using System.Collections;
using FMODUnity;
using Interfaces.AI.Combat;
using UnityEngine;

namespace AI.Combat.AbilityProjectiles
{
    public class Projectile: MonoBehaviour
    {
        [SerializeField] private Collider _collider;
        [SerializeField] private Rigidbody _rigidbody;

        private EventReference _projectileSound;
        
        private float _speed;

        private float _timeToVanish;

        private Action _onFireAction = () => { };
        private Action _onVanishAction = () => { };
        
        private IAbilityCollider _abilityCollider;

        public void SetProjectileSpecs(float projectileSpeed, float timeToVanish, bool doesExplodeOnVanish, EventReference projectileSound)
        {
            _projectileSound = projectileSound;
            
            _speed = projectileSpeed;

            if (timeToVanish == 0)
            {
                return;
            }
            
            _timeToVanish = timeToVanish;
            _onFireAction = () => StartCoroutine(VanishTimeCoroutine());

            if (!doesExplodeOnVanish)
            {
                return;
            }

            _onVanishAction = BOOOOOOM;
        }

        public void SetAbilityCollider(IAbilityCollider abilityColliderCollider)
        {
            _abilityCollider = abilityColliderCollider;
        }

        public void ResetProjectile(Transform parentTransform, Vector3 relativePosition)
        {
            _collider.enabled = false;
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            
            Transform ownTransform = transform;
            ownTransform.parent = parentTransform;
            ownTransform.localPosition = relativePosition;
            ownTransform.localRotation = Quaternion.identity;
        }

        public void FIREEEEEEEEEEEE(Vector3 forceVector)
        {
            AudioManager.Instance.PlayOneShot(_projectileSound, transform.position);
            transform.rotation = Quaternion.LookRotation(forceVector.normalized);
            gameObject.SetActive(true);
            transform.parent = null;
            _rigidbody.AddForce(forceVector, ForceMode.VelocityChange);
            _collider.enabled = true;
            _onFireAction();
        }

        private void BOOOOOOM()
        {
            _abilityCollider.Activate();
            gameObject.SetActive(false);
        }

        private IEnumerator VanishTimeCoroutine()
        {
            float timer = 0;

            while (timer < _timeToVanish)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            gameObject.SetActive(false);

            _onVanishAction();
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