using System;
using System.Collections.Generic;
using AI.Combat.AbilityAoEColliders;
using AI.Combat.AbilityCasts;
using AI.Combat.AbilityProjectiles;
using AI.Combat.AbilitySpecs;
using AI.Combat.ScriptableObjects;
using ECS.Components.AI.Combat.Abilities;
using ECS.Entities;
using Interfaces.AI.Combat;
using UnityEngine;

namespace Managers
{
    public class AbilityManager : MonoBehaviour
    {
        private static AbilityManager _instance;

        public static AbilityManager Instance => _instance;

        [SerializeField] private GameObject _enemyRectangleAttackColliderPrefab;
        [SerializeField] private GameObject _enemyCircleAttackColliderPrefab;

        /*private readonly Dictionary<AbilityAoEType, Delegate> _colliderFactories = new Dictionary<AbilityAoEType, Delegate>
            {
                { AbilityAoEType.RECTANGULAR , 
                    new Func<BasicAbilityComponent, RectangularAbilityComponent, Transform, EntityType, RectangularAbilityAoECollider>(
                        (basicAbilityComponent, rectangularAbilityComponent, parentTransform, entityType) => 
                    _instance.InstantiateAbilityCollider<RectangularAbilityComponent, RectangularAbilityAoECollider>
                        (basicAbilityComponent, rectangularAbilityComponent, parentTransform, entityType)) },
                
                { AbilityAoEType.SPHERICAL , 
                    new Func<BasicAbilityComponent, SphericalAbilityComponent, Transform, EntityType, SphericalAbilityAoECollider>(
                        (basicAbilityComponent, sphericalAbilityComponent, parentTransform, entityType) => 
                    _instance.InstantiateAbilityCollider<SphericalAbilityComponent, SphericalAbilityAoECollider>
                        (basicAbilityComponent, sphericalAbilityComponent, parentTransform, entityType)) },
                
                
                { AbilityAoEType.CONICAL , 
                    new Func<BasicAbilityComponent, ConicalAbilityComponent, Transform, EntityType, ConicalAbilityAoECollider>(
                        (basicAbilityComponent, abilityComponent, parentTransform, entityType) => 
                    _instance.InstantiateAbilityCollider<ConicalAbilityComponent, ConicalAbilityAoECollider>
                        (basicAbilityComponent, abilityComponent, parentTransform, entityType)) },
                
                
                { AbilityAoEType.CUSTOM_MESH , 
                    new Func<BasicAbilityComponent, CustomMeshAbilityComponent, Transform, EntityType, CustomMeshAbilityAoECollider>(
                        (basicAbilityComponent, abilityComponent, parentTransform, entityType) => 
                    _instance.InstantiateAbilityCollider<CustomMeshAbilityComponent, CustomMeshAbilityAoECollider>
                        (basicAbilityComponent, abilityComponent, parentTransform, entityType)) }
            };*/

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                
                DontDestroyOnLoad(gameObject);
                
                return;
            }
            
            Destroy(gameObject);
        }

        #region Ability Creation

        public BasicAbility CreateBasicAbility(BasicAbilityProperties basicAbilityProperties)
        {
            BasicAbilityComponent basicAbilityComponent = new BasicAbilityComponent(basicAbilityProperties);

            return new BasicAbility(basicAbilityComponent);
        }

        #region Area

        public IAreaAbility ReturnAreaAbility(AreaAbilityProperties areaAbilityProperties, Transform parentTransform)
        {
            BasicAbilityComponent basicAbilityComponent = new BasicAbilityComponent(areaAbilityProperties);
            
            IAreaAbility areaAbility = null;
            
            switch (areaAbilityProperties.abilityAoEType)
            {
                case AbilityAoEType.RECTANGULAR:
                    RectangularAbilityComponent rectangularAbilityComponent = new RectangularAbilityComponent(areaAbilityProperties);

                    areaAbility = CreateAreaAbility<RectangularAbilityComponent, RectangularAbilityAoECollider>(
                            basicAbilityComponent, rectangularAbilityComponent, parentTransform);
                    break;
                
                case AbilityAoEType.SPHERICAL:
                    SphericalAbilityComponent sphericalAbilityComponent = new SphericalAbilityComponent(areaAbilityProperties);

                    areaAbility = CreateAreaAbility<SphericalAbilityComponent, SphericalAbilityAoECollider>(
                            basicAbilityComponent, sphericalAbilityComponent, parentTransform);
                    break;
                
                case AbilityAoEType.CONICAL:
                    ConicalAbilityComponent conicalAbilityComponent = new ConicalAbilityComponent(areaAbilityProperties);

                    areaAbility = CreateAreaAbility<ConicalAbilityComponent, ConicalAbilityAoECollider>(
                            basicAbilityComponent, conicalAbilityComponent, parentTransform);
                    break;
                
                case AbilityAoEType.CUSTOM_MESH:
                    CustomMeshAbilityComponent customMeshAbilityComponent = new CustomMeshAbilityComponent(areaAbilityProperties);

                    areaAbility = CreateAreaAbility<CustomMeshAbilityComponent, CustomMeshAbilityAoECollider>(
                            basicAbilityComponent, customMeshAbilityComponent, parentTransform);
                    break;
            }
            
            return areaAbility;
        }

        private AreaAbility<TAreaAbilityComponent, TAbilityCollider> CreateAreaAbility<TAreaAbilityComponent, TAbilityCollider>
            (BasicAbilityComponent basicAbilityComponent, TAreaAbilityComponent areaAbilityComponent, Transform parentTransform)
                where TAbilityCollider : AbilityAoECollider<TAreaAbilityComponent>
                where TAreaAbilityComponent : AreaAbilityComponent
        {
            TAbilityCollider abilityCollider =
                InstantiateAbilityCollider<TAreaAbilityComponent, TAbilityCollider>(basicAbilityComponent, 
                    areaAbilityComponent, parentTransform);

            return new AreaAbility<TAreaAbilityComponent, TAbilityCollider>(basicAbilityComponent, abilityCollider);
        }

        #endregion

        #region Projectile

        public IProjectileAbility ReturnProjectileAbility(ProjectileAbilityProperties projectileAbilityProperties, 
            Transform parentTransform)
        {
            BasicAbilityComponent basicAbilityComponent = new BasicAbilityComponent(projectileAbilityProperties);

            IProjectileAbility projectileAbility = null;

            switch (projectileAbilityProperties.abilityAoEType)
            {
                case AbilityAoEType.RECTANGULAR:
                    RectangularAbilityComponent rectangularAbilityComponent = 
                        new RectangularAbilityComponent(projectileAbilityProperties);

                    projectileAbility = CreateProjectileAbility<RectangularAbilityComponent, RectangularAbilityAoECollider>(
                            basicAbilityComponent, rectangularAbilityComponent, projectileAbilityProperties.abilityProjectile,
                            parentTransform);
                    break;
                
                case AbilityAoEType.SPHERICAL:
                    SphericalAbilityComponent sphericalAbilityComponent = 
                        new SphericalAbilityComponent(projectileAbilityProperties);

                    projectileAbility = CreateProjectileAbility<SphericalAbilityComponent, SphericalAbilityAoECollider>(
                            basicAbilityComponent, sphericalAbilityComponent, projectileAbilityProperties.abilityProjectile,
                            parentTransform);
                    break;
                
                case AbilityAoEType.CONICAL:
                    ConicalAbilityComponent conicalAbilityComponent =
                        new ConicalAbilityComponent(projectileAbilityProperties);

                    projectileAbility = CreateProjectileAbility<ConicalAbilityComponent, ConicalAbilityAoECollider>(
                            basicAbilityComponent, conicalAbilityComponent, projectileAbilityProperties.abilityProjectile,
                            parentTransform);
                    break;
                
                case AbilityAoEType.CUSTOM_MESH:
                    CustomMeshAbilityComponent customMeshAbilityComponent =
                        new CustomMeshAbilityComponent(projectileAbilityProperties);
                    
                    projectileAbility = CreateProjectileAbility<CustomMeshAbilityComponent, CustomMeshAbilityAoECollider>(
                            basicAbilityComponent, customMeshAbilityComponent, projectileAbilityProperties.abilityProjectile,
                            parentTransform);
                    break;
            }

            return projectileAbility;
        }

        private ProjectileAbility CreateProjectileAbility<TAreaAbilityComponent, TAbilityCollider>(
            BasicAbilityComponent basicAbilityComponent, TAreaAbilityComponent areaAbilityComponent,
            AbilityProjectile abilityProjectile, Transform parentTransform)
                where TAbilityCollider : AbilityAoECollider<TAreaAbilityComponent>
                where TAreaAbilityComponent : AreaAbilityComponent
        {
            List<Projectile> projectiles = new List<Projectile>();

            for (int i = 0; i < abilityProjectile.instances; i++)
            {
                projectiles.Add(InstantiateProjectile<TAreaAbilityComponent, TAbilityCollider>(basicAbilityComponent, 
                    areaAbilityComponent, abilityProjectile.projectileSpeed, abilityProjectile.projectilePrefab, 
                    abilityProjectile.objectWithParticleSystem, abilityProjectile.makesParabola));
            }

            return new ProjectileAbility(basicAbilityComponent, projectiles, parentTransform, 
                abilityProjectile.relativePositionToCaster, abilityProjectile.maximumDispersion, abilityProjectile.makesParabola);
        }

        #endregion

        #endregion

        private Projectile InstantiateProjectile<TAreaAbilityComponent, TAbilityCollider>
            (BasicAbilityComponent basicAbilityComponent, TAreaAbilityComponent areaAbilityComponent, float projectileSpeed, 
                GameObject projectilePrefab, GameObject particleObjectPrefab, bool makesAParabola)
                where TAbilityCollider : AbilityAoECollider<TAreaAbilityComponent>
                where TAreaAbilityComponent : AreaAbilityComponent
        {
            GameObject projectileObject = Instantiate(projectilePrefab);

            Instantiate(particleObjectPrefab, projectileObject.transform);
            
            Projectile projectile = projectileObject.GetComponent<Projectile>();

            projectile.SetProjectileSpecs(projectileSpeed, makesAParabola);
            
            projectilePrefab.SetActive(false);

            projectile.SetAbilityCollider(InstantiateAbilityCollider<TAreaAbilityComponent, TAbilityCollider>(
                    basicAbilityComponent, areaAbilityComponent, projectile.transform));

            return projectile;
        }

        private TAbilityCollider InstantiateAbilityCollider<TAreaAbilityComponent, TAbilityCollider>
            (BasicAbilityComponent basicAbilityComponent, TAreaAbilityComponent areaAbilityComponent, 
                Transform parentTransform)
                where TAbilityCollider : AbilityAoECollider<TAreaAbilityComponent>
                where TAreaAbilityComponent : AreaAbilityComponent
        {
            GameObject colliderObject = areaAbilityComponent.GetAoEType() == AbilityAoEType.CUSTOM_MESH
                ? areaAbilityComponent.GetAoE().customMeshPrefab
                : Instantiate(ReturnPrefab(areaAbilityComponent.GetAoEType()));
            
            TAbilityCollider abilityCollider = colliderObject.GetComponent<TAbilityCollider>();
            
            abilityCollider.SetAbilitySpecs(parentTransform, basicAbilityComponent, areaAbilityComponent);

            EntityType abilityTargets = basicAbilityComponent.GetTargets();

            abilityCollider.SetAbilityTargets(ReturnTargets(abilityTargets));
            
            colliderObject.SetActive(false);

            return abilityCollider;
        }

        private List<EntityType> ReturnTargets(EntityType targets)
        {
            List<EntityType> abilityTargets = new List<EntityType>();
            
            if ((targets & EntityType.PLAYER) != 0)
            {
                abilityTargets.Add(EntityType.PLAYER);
            }
            
            if ((targets & EntityType.TRIFACE) != 0)
            {
                abilityTargets.Add(EntityType.TRIFACE);
            }
            
            if ((targets & EntityType.LONG_ARMS) != 0)
            {
                abilityTargets.Add(EntityType.LONG_ARMS);
            }
            
            if ((targets & EntityType.LONG_ARMS_BASE) != 0)
            {
                abilityTargets.Add(EntityType.LONG_ARMS_BASE);
            }
            
            if ((targets & EntityType.SENDATU) != 0)
            {
                abilityTargets.Add(EntityType.SENDATU);
            }

            return abilityTargets;
        }

        private GameObject ReturnPrefab(AbilityAoEType abilityAoEType)
        {
            switch (abilityAoEType)
            {
                case AbilityAoEType.RECTANGULAR:
                    return _enemyRectangleAttackColliderPrefab;
                
                case AbilityAoEType.SPHERICAL:
                case AbilityAoEType.CONICAL:
                    return _enemyCircleAttackColliderPrefab;
            }

            return null;
        }

        private TReturn ExecuteDelegate<TReturn, TCollection, TAbilityComponent>(TCollection collection, 
            AbilityAoEType abilityAoEType, TAbilityComponent abilityComponent, Transform transform, EntityType entityType)
                where TCollection : Dictionary<AbilityAoEType, Delegate> 
        {
            Delegate del = collection[abilityAoEType];

            if (del is Func<TAbilityComponent, Transform, EntityType, TReturn> func)
            {
                return func(abilityComponent, transform, entityType);
            }

            return default;
        }
    }
}