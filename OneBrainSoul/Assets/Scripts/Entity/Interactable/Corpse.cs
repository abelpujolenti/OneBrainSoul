using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

public class Corpse : Item
{
    protected override void Pickup(GameObject collider)
    {
        base.Pickup(collider);
        PlayerCharacter player = collider.GetComponent<PlayerCharacter>();
        player.RecoverBody();
    }
}
