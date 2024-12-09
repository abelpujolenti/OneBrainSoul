using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class HealerEnemy : EnemyBase
{
    [SerializeField] protected int healAmount = 1;
    [SerializeField] protected float healCooldown = 5f;
    [SerializeField] protected Transform projectile;
    protected float healTime = 0f;
    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        healTime = Mathf.Max(0f, healTime - Time.deltaTime);
    }

    protected override void BehaviorUpdate()
    {
        EnemyBase followedEnemy = null;
        foreach (DamageTakingEntity entity in ActiveDamageTakingEntityManager.Instance.damageTakingEntities)
        {
            EnemyBase enemy = entity.GetComponent<EnemyBase>();
            if (enemy == null) continue;
            if (enemy == this) continue;
            float minD = float.PositiveInfinity;
            float d = Vector3.Distance(enemy.transform.position, transform.position);
            followedEnemy = d < minD && d < range ? enemy : followedEnemy;
            minD = d < minD ? d : minD;
        }
        if (healTime == 0f && followedEnemy != null)
        {
            LaunchProjectile(followedEnemy);
            healTime = healCooldown;
        }
    }

    void LaunchProjectile(EnemyBase entity)
    {
        Vector3 dir = (entity.transform.position - transform.position).normalized;
        VisualProjectile p = Instantiate(projectile, transform.position + new Vector3(dir.x, 0f, dir.z).normalized * 2.5f, Quaternion.identity).GetComponent<VisualProjectile>();
        p.Init(transform, entity.transform, 25f, Heal);
    }

    void Heal(GameObject entity)
    {
        entity.GetComponent<EnemyBase>().Heal(this, healAmount);
    }
}
