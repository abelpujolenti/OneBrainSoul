using Player;
using UnityEngine;

public class DashItem : Item
{
    protected override void Pickup(GameObject collider)
    {
        collider.GetComponent<PlayerCharacterController>().UnlockDash();
        base.Pickup(collider);
    }
}
