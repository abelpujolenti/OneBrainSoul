using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : EnemyBase
{
    protected override void Start()
    {
        base.Start();
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
        }
    }
    void FollowUpdate(PlayerCharacterController player)
    {
        Vector3 plVector = (player.transform.position - transform.position);
        plVector.y = 0f;
        if (plVector.magnitude > radius + 2f && damageCooldown <= 0)
        {
            transform.rotation = Quaternion.LookRotation(plVector.normalized);
            rb.AddForce(plVector.normalized * speed, ForceMode.Acceleration);
        }
    }
}
