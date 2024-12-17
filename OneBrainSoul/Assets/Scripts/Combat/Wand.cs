using System.Collections.Generic;
using UnityEngine;

public class Wand : Weapon
{
    [SerializeField] WandProjectile projectile;
    public float projectileSpeed = 10f;

    protected override void AttackUpdate()
    {
        base.AttackUpdate();
        if (!attackLanded && animationTimer <= 1 - activeStart && animationTimer >= 1 - activeEnd)
        {
            ShootProjectile();
        }
    }

    protected override void AttackCommand()
    {
        base.AttackCommand();
    }

    protected override void AttackLand(List<DamageTakingEntity> entities)
    {
        player.hitstop.Add(hitstop * (.8f + entities.Count * .2f));
    }

    public void ProjectileLanded(WandProjectile wandProjectile, List<DamageTakingEntity> entities)
    {
        AttackLand(entities);
    }

    private void ShootProjectile()
    {
        attackLanded = true;
        WandProjectile shotProjectile = Instantiate(projectile.transform, player.transform.position + new Vector3(0f, 1f, 0f), Quaternion.identity).GetComponent<WandProjectile>();
        shotProjectile.Init(player, this, range, player.cam.transform.forward, projectileSpeed);
        AudioManager.instance.PlayOneShot(FMODEvents.instance.wandAttack, player.transform.position);
    }
}
