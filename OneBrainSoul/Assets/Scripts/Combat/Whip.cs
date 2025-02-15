using System.Collections.Generic;
using ECS.Entities.AI;
using Managers;
using UnityEngine;

namespace Combat
{
    public class Whip : Weapon
    {
        protected override void AttackCommand()
        {
            base.AttackCommand();
            AudioManager.instance.PlayOneShot(FMODEvents.instance.whipAttack, player.transform.position);
        }

        protected override void AttackUpdate()
        {
            base.AttackUpdate();

            if (!attackLanded && animationTimer <= 1 - activeStart && animationTimer >= 1 - activeEnd)
            {
                List<AgentEntity> entities = CombatManager.Instance.ReturnAllEnemies();
                List<AgentEntity> affectedEntities = new List<AgentEntity>();
                for (int i = 0; i < entities.Count; i++)
                {
                    AgentEntity entity = entities[i];
                    Vector3 enemyPos = entity.transform.position;
                    Vector3 enemyPosOuter = entity.transform.position + (player.transform.position - entity.transform.position).normalized * entity.GetRadius();
                    float distance = Vector3.Distance(player.transform.position, enemyPosOuter);
                    float dotInnerNormalized = Vector3.Dot(player.GetOrientation().forward, (enemyPos - player.transform.position).normalized);
                    float dotOuterNormalized = Vector3.Dot(player.GetOrientation().forward, (enemyPosOuter - player.transform.position).normalized);
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
    }
}
