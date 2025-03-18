using Player;
using UnityEngine;

public class RespawnItem : Item
{
    protected override void Pickup(GameObject collider)
    {
        collider.GetComponent<PlayerCharacterController>().SetRespawn(transform.position - Vector3.up * 0.5f);
        base.Pickup(collider);
    }
}
