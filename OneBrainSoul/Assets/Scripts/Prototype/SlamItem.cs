using Player;
using UnityEngine;

public class SlamItem : Item
{
    protected override void Pickup(GameObject collider)
    {
        collider.GetComponent<PlayerCharacterController>().UnlockCharge();
        base.Pickup(collider);
    }
}
