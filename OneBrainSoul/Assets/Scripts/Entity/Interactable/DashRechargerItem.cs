using Player;
using UnityEngine;

public class DashRechargerItem : Item
{
    protected override void Pickup(GameObject collider)
    {
        collider.GetComponent<PlayerCharacterController>().AddHookCharge(3);
        base.Pickup(collider);
    }
}
