using UnityEngine;

public class RangedEnemy : EnemyBase
{
    [SerializeField] protected float projectileSpeed = 2000f;
    [SerializeField] protected float projectileCooldown = 5f;
    [SerializeField] protected Transform projectile;
    protected float shootTime = 0f;
    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        shootTime = Mathf.Max(0f, shootTime - Time.deltaTime);
    }

    protected override void BehaviorUpdate()
    {
        PlayerCharacterController followedPlayer = null;
        foreach (PlayerCharacterController player in playerCharacters)
        {

            float minD = float.PositiveInfinity;
            float d = Vector3.Distance(player.transform.position, transform.position);
            followedPlayer = d < minD && d < range ? player : followedPlayer;
            minD = d < minD ? d : minD;
        }
        if (shootTime == 0f && followedPlayer != null)
        {
            LaunchProjectile(followedPlayer);
            shootTime = projectileCooldown;
        }
    }

    void LaunchProjectile(PlayerCharacterController player)
    {
        Vector3 dir = (player.transform.position - transform.position).normalized;
        TestEnemyProjectile proj = Instantiate(projectile, transform.position + new Vector3(dir.x, 0f, dir.z).normalized * 2.5f, Quaternion.identity).GetComponent<TestEnemyProjectile>();
        proj.Init(2f, dir, projectileSpeed, damage);
    }
}
