using System.Collections.Generic;
using ECS.Entities.AI;
using Managers;
using UnityEngine;

namespace Combat
{
    public class Hammer : Weapon
    {
        protected override void AttackCommand()
        {
            base.AttackCommand();
            AudioManager.instance.PlayOneShot(FMODEvents.instance.hammerAttack, player.transform.position);
            player.GetAnimator().SetBool("Attack", true);
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
                    if (entity == null) continue;
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

        protected override void AttackEnd()
        {
            base.AttackEnd();
            player.GetAnimator().SetBool("Attack", false);
        }

        protected override void AttackLand(List<AgentEntity> affectedEntities)
        {
            base.AttackLand(affectedEntities);
            if (player.IsOnTheGround())
            {
                return;
            }

            player.GetCamera().ScreenShake(.1f, .4f);
            _hitstop.Add(.025f);
            Vector3 pushVector = player.GetOrientation().forward;
            pushVector.y = 1;
            pushVector = pushVector.normalized;
            float forceMagnitude = 300f; 
        
            foreach (AgentEntity enemy in affectedEntities)
            {
                if (enemy == null) continue;
                enemy.OnReceivePushFromCenter(transform.position, pushVector, forceMagnitude, transform.position, 
                    ForceMode.VelocityChange);    
            }
            //Does extra dmg and whatever
        }
    }
}
