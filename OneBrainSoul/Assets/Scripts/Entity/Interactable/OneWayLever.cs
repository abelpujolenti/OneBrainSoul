using UnityEngine;

public class OneWayLever : Item
{
    [SerializeField] Door door;

    protected override void Pickup(GameObject collider)
    {
        door.Open();
        base.Pickup(collider);
    }
}
