using Player;
using UnityEngine;

public class HookItem : Item
{
    protected override void Pickup(GameObject collider)
    {
        collider.GetComponent<PlayerCharacterController>().UnlockHook();
        base.Pickup(collider);
    }
}
