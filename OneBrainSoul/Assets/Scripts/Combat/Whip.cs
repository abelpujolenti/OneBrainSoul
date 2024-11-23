using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whip : Weapon
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
            List<EnemyTest> affectedEnemies = new List<EnemyTest>();
            for (int i = 0; i < activeEnemyManager.activeEnemies.Count; i++)
            {
                EnemyTest enemy = activeEnemyManager.activeEnemies[i];
                Vector3 enemyPos = enemy.transform.position;
                Vector3 enemyPosOuter = enemy.transform.position + (player.transform.position - enemy.transform.position).normalized * enemy.radius;
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
                        affectedEnemies.Add(enemy);
                    }
                }
            }
            if (affectedEnemies.Count > 0)
            {
                AttackLand(affectedEnemies);
            }
        }
    }

    protected override void AttackLand(List<EnemyTest> enemies)
    {
        base.AttackLand(enemies);


    }
}
