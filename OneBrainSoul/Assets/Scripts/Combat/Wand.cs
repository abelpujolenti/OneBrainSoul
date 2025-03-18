using System.Collections.Generic;
using ECS.Entities.AI;
using UnityEngine;

namespace Combat
{
    public class Wand : Weapon
    {
        [SerializeField] private WandProjectile wandProjectilePrefab;
        private float projectileSpeed = 10f;

        [SerializeField] private uint _maxProjectiles;
        private WandProjectile[] _wandProjectilesPool;
        private uint _counter;

        private void Start()
        {
            _wandProjectilesPool = new WandProjectile[_maxProjectiles];
            for (int i = 0; i < _maxProjectiles; i++)
            {
                _wandProjectilesPool[i] = Instantiate(wandProjectilePrefab,
                    player.transform.position + new Vector3(0f, 1f, 0f) + player.GetCamera().transform.forward * 2f,
                    Quaternion.identity).GetComponent<WandProjectile>();
            
                _wandProjectilesPool[i].Init(range, projectileSpeed);
            }
        }

        protected override void AttackUpdate()
        {
            base.AttackUpdate();
            if (!attackLanded && animationTimer <= 1 - activeStart && animationTimer >= 1 - activeEnd)
            {
                ShootProjectile();
            }
        }

        protected override void AttackLand(List<AgentEntity> affectedEntities)
        {
            _hitstop.Add(hitstop * (.8f + affectedEntities.Count * .2f));
        }

        private void ShootProjectile()
        {
            attackLanded = true;
            WandProjectile wandProjectile = _wandProjectilesPool[_counter];
            wandProjectile.gameObject.SetActive(true);
            wandProjectile.Shoot(player.IsOnTheGround(), player.GetCamera().transform.forward);
            _counter = (_counter + 1) % _maxProjectiles; 
        
            AudioManager.instance.PlayOneShot(FMODEvents.instance.wandAttack, player.transform.position);
        }
    }
}
