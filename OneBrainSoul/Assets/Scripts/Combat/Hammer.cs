using System.Collections.Generic;
using UnityEngine;

public class Hammer : Weapon
{
    protected override void AttackCommand()
    {
        base.AttackCommand();
    }

    protected override void AttackUpdate()
    {
        base.AttackUpdate();

        if (!attackLanded && animationTimer <= 1 - activeStart && animationTimer >= 1 - activeEnd)
        {
            List<DamageTakingEntity> affectedEntities = new List<DamageTakingEntity>();
            for (int i = 0; i < ActiveDamageTakingEntityManager.Instance.damageTakingEntities.Count; i++)
            {
                DamageTakingEntity entity = ActiveDamageTakingEntityManager.Instance.damageTakingEntities[i];
                Vector3 enemyPos = entity.transform.position;
                Vector3 enemyPosOuter = entity.transform.position + (player.transform.position - entity.transform.position).normalized * entity.radius;
                float distance = Vector3.Distance(player.transform.position, enemyPosOuter);
                float dotInnerNormalized = Vector3.Dot(player.orientation.forward, (enemyPos - player.transform.position).normalized);
                float dotOuterNormalized = Vector3.Dot(player.orientation.forward, (enemyPosOuter - player.transform.position).normalized);
                if (distance < range)
                {
                    //Debug.Log("Dot inner: " + dotInnerNormalized + "  Dot outer: " + dotOuterNormalized);
                    if (
                        (dotOuterNormalized > 1f - outerArc / 180f) ||
                        (dotInnerNormalized > 0.2f && distance < innerRange)
                        )
                    {
                        affectedEntities.Add(entity);
                    }
                }
            }
            if (affectedEntities.Count > 0)
            {
                AttackLand(affectedEntities);
            }
        }
    }

    protected override void AttackLand(List<DamageTakingEntity> enemies)
    {
        base.AttackLand(enemies);

        if (!player.onGround)
        {
            player.cam.ScreenShake(.1f, .4f);
            player.hitstop.Add(.025f);
            foreach (DamageTakingEntity enemy in enemies)
            {
                if (enemy is EnemyBase)
                {
                    (enemy as EnemyBase).Knockback(player);
                }
            }
            //Does extra dmg and whatever
        }
    }
}
