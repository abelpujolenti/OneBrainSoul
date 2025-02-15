using System;
using System.Collections.Generic;
using AI.Combat.AbilityProjectiles;
using AI.Combat.AbilitySpecs;
using ECS.Components.AI.Combat.Abilities;
using ECS.Entities.AI;
using Interfaces.AI.Combat;
using Managers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AI.Combat.AbilityCasts
{
    public class ProjectileAbility : IProjectileAbility
    {
        private BasicAbilityComponent _basicAbilityComponent;
        private Queue<Projectile> _projectilesPool = new Queue<Projectile>();
        private Projectile _currentProjectile;

        private Transform _parentTransform;

        private Vector3 _relativePosition;

        private float _maximumDispersion;
        
        private uint _targetId;
        
        private Func<Projectile, Vector3> _actionProjectileLaunch;

        public ProjectileAbility(BasicAbilityComponent basicAbilityComponent, List<Projectile> projectiles, 
            Transform parentTransform, Vector3 relativePosition, float maximumDispersion, bool makesParabola)
        {
            _basicAbilityComponent = basicAbilityComponent;

            foreach (Projectile projectile in projectiles)
            {
                _projectilesPool.Enqueue(projectile);
            }
            
            _parentTransform = parentTransform;
            _relativePosition = relativePosition;
            _maximumDispersion = maximumDispersion;
            
            if (makesParabola)
            {
                _actionProjectileLaunch = projectile =>
                {
                    AgentEntity target = CombatManager.Instance.ReturnAgentEntity(_targetId);
                    return CalculateParabolicForceVector(target.GetTransformComponent().GetPosition(),
                        target.GetVelocity(), projectile.transform.position, projectile.GetSpeed(), 9.8f);
                };
                return;
            }

            _actionProjectileLaunch = projectile =>
            {
                AgentEntity target = CombatManager.Instance.ReturnAgentEntity(_targetId);
                return CalculateLinearForceVector(target.GetTransformComponent().GetPosition(), target.GetVelocity(), 
                    projectile.transform.position, projectile.GetSpeed(), basicAbilityComponent.GetCast().maximumRangeToCast, 
                    _maximumDispersion);
            };

        }

        public void Activate()
        {
            _currentProjectile = _projectilesPool.Dequeue();
            _currentProjectile.ResetProjectile(_parentTransform, _relativePosition);
            _currentProjectile.gameObject.SetActive(true);
            _projectilesPool.Enqueue(_currentProjectile);
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

        public bool FIREEEEEEEEEEEEEE()
        {
            Vector3 forceVector = _actionProjectileLaunch(_currentProjectile);

            if (forceVector == Vector3.zero)
            {
                _currentProjectile.gameObject.SetActive(false);
                return false;
            }
            
            _currentProjectile.FIREEEEEEEEEEEE(forceVector);
            return true;
        }

        private static Vector3 CalculateLinearForceVector(Vector3 targetPosition, Vector3 targetVelocity, Vector3 ownPosition,
            float projectileSpeed, float maximumDistance, float maximumDispersion)
        {
            Vector3 dispersionVector = CalculateDispersion(targetPosition - ownPosition, maximumDistance, maximumDispersion);

            targetPosition += dispersionVector;
            
            Vector3 vectorToTarget = targetPosition - ownPosition;
            
            if (targetVelocity.magnitude < 0.01f)
            {
                return vectorToTarget.normalized * projectileSpeed;
            }
            
            float a = Vector3.Dot(targetVelocity, targetVelocity) - projectileSpeed * projectileSpeed;
            float b = 2 * Vector3.Dot(targetVelocity, vectorToTarget);
            float c = Vector3.Dot(vectorToTarget, vectorToTarget);

            float discriminant = b * b - 4 * a * c;

            if (discriminant < 0)
            {
                return Vector3.zero;
            }

            float sqrtDiscriminant = Mathf.Sqrt(discriminant);
            float t1 = (-b + sqrtDiscriminant) / (2 * a);
            float t2 = (-b - sqrtDiscriminant) / (2 * a);

            float time = Mathf.Max(t1, t2);
            
            if (time < 0)
            {
                return Vector3.zero;
            }

            Vector3 interceptionPoint = targetPosition + targetVelocity * time;
            return (interceptionPoint - ownPosition).normalized * projectileSpeed;
        }
        
        private static Vector3 CalculateParabolicForceVector(Vector3 targetPosition, Vector3 targetVelocity, Vector3 ownPosition,
            float projectileSpeed, float gravity)
        {
            Vector3 vectorToTarget = targetPosition - ownPosition;
            
            float a = -0.5f * gravity;
            float b = targetVelocity.y;
            float c = vectorToTarget.y;

            float discriminant = b * b - 4 * a * c;

            if (discriminant < 0)
            {
                return Vector3.zero;
            }

            float sqrtDiscriminant = Mathf.Sqrt(discriminant);
            float t1 = (-b + sqrtDiscriminant) / (2 * a);
            float t2 = (-b - sqrtDiscriminant) / (2 * a);

            float time = Mathf.Max(t1, t2);
            
            if (time < 0)
            {
                return Vector3.zero;
            }

            Vector3 forceVector = 
                (vectorToTarget + targetVelocity * time - Vector3.up * (0.5f * gravity * time * time)) / time;

            return forceVector.magnitude > projectileSpeed ? Vector3.zero : forceVector.normalized * projectileSpeed;
        }

        private static Vector3 CalculateDispersion(Vector3 vectorToTarget, float maximumDistance, float maximumDispersion)
        {
            Vector3 randomPerpendicular = Vector3.Cross(vectorToTarget, Vector3.up).normalized;
            
            Quaternion randomRotation = Quaternion.AngleAxis(Random.Range(0f, 360f), vectorToTarget);
            Vector3 randomDeviation = randomRotation * randomPerpendicular;

            return randomDeviation * (Random.Range(0, (vectorToTarget.magnitude * maximumDispersion) / maximumDistance));
        }
    }
}