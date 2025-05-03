using System;
using System.Collections.Generic;
using AI.Combat.AbilityProjectiles;
using AI.Combat.AbilitySpecs;
using Interfaces.AI.Combat;
using UnityEngine;

namespace AI.Combat.AbilityCasts
{
    public class ProjectileAbility : IProjectileAbility
    {
        private AbilityCast _abilityCast;
        private Queue<Projectile> _projectilesPool = new Queue<Projectile>();
        private Projectile _currentProjectile;

        private Transform _parentTransform;

        private Vector3 _relativePosition;

        private float _dispersionRatePer1Meter;
        
        private uint _targetId;
        
        private Func<Projectile, Vector3> _actionProjectileLaunch;

        public ProjectileAbility(AbilityCast abilityCast, List<Projectile> projectiles, Transform parentTransform, 
            Vector3 relativePosition)
        {
            _abilityCast = abilityCast;

            foreach (Projectile projectile in projectiles)
            {
                _projectilesPool.Enqueue(projectile);
            }
            
            _parentTransform = parentTransform;
            _relativePosition = relativePosition;
        }

        public Projectile Activate()
        {
            _currentProjectile = _projectilesPool.Dequeue();
            _currentProjectile.ResetProjectile(_parentTransform, _relativePosition);
            _currentProjectile.gameObject.SetActive(true);
            _projectilesPool.Enqueue(_currentProjectile);

            return _currentProjectile;
        }

        public void Cancel()
        {
            _currentProjectile.gameObject.SetActive(false);
        }

        public AbilityCast GetCast()
        {
            return _abilityCast;
        }

        public void SetTargetId(uint targetId)
        {
            _targetId = targetId;
        }

        public uint GetTargetId()
        {
            return _targetId;
        }

        public void FIREEEEEEEEEEEEEE(Vector3 forceVector)
        {
            _currentProjectile.FIREEEEEEEEEEEE(forceVector.normalized * _currentProjectile.GetSpeed());
        }
    }
}