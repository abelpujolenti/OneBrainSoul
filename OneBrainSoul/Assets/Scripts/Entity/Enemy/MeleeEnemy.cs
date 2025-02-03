using Player;
using UnityEngine;

public class MeleeEnemy : EnemyBase
{
    [SerializeField] protected float attackCooldown = 2f;
    [SerializeField] protected float attackRange = 2f;
    protected float attackTime = 0f;
    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        attackTime = Mathf.Max(0f, attackTime - Time.deltaTime);
    }

    protected override void BehaviorUpdate()
    {
        base.BehaviorUpdate();
        PlayerCharacterController followedPlayer = null;
        foreach (PlayerCharacterController player in playerCharacters)
        {

            float minD = float.PositiveInfinity;
            float d = Vector3.Distance(player.transform.position, transform.position);
            followedPlayer = d < minD && d < range ? player : followedPlayer;
            minD = d < minD ? d : minD;
        }
        if (followedPlayer != null)
        {
            FollowUpdate(followedPlayer);
            if (attackTime == 0f)
            {
                Attack(followedPlayer);
            }
        }
    }

    void Attack(PlayerCharacterController player)
    {
        if (Vector3.Distance(player.transform.position, transform.position) < attackRange) {
            //if (player.health.Damage(damage, gameObject))
            {
                attackTime = attackCooldown;
                AudioManager.instance.PlayOneShot(FMODEvents.instance.enemyAttack, transform.position);
            }
        }
    }

    void FollowUpdate(PlayerCharacterController player)
    {
        /*Vector3 plVector = (player.transform.position - transform.position);
        plVector.y = 0f;
        if (plVector.magnitude > radius + 2f && damageCooldown <= 0)
        {
            transform.rotation = Quaternion.LookRotation(plVector.normalized);
            float s = speed * (attackTime > 0f ? .4f : 1f);
            rb.AddForce(plVector.normalized * s, ForceMode.Acceleration);

            Vector3 horizontalVelocity = rb.velocity;
            horizontalVelocity.y = 0;
            rb.AddForce(-horizontalVelocity * 15f, ForceMode.Acceleration);
        }*/
    }
}
