using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookItem : Item
{
    protected override void Pickup(GameObject collider)
    {
        collider.GetComponent<PlayerCharacterController>().UnlockHook();
        base.Pickup(collider);
    }
}
