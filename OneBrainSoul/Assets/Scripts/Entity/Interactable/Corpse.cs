using Player;
using UnityEngine;

public class Corpse : Item
{
    protected override void TryPickup(GameObject collider)
    {
        PlayerCharacter player = collider.GetComponent<PlayerCharacter>();
        if (player.GetGhostTimeNormalized() < 0.05f) return;
        base.TryPickup(collider);
    }
    protected override void Pickup(GameObject collider)
    {
        base.Pickup(collider);
        PlayerCharacter player = collider.GetComponent<PlayerCharacter>();
        player.RecoverBody();
    }
}
