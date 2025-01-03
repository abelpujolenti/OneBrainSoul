using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashItem : Item
{
    protected override void Pickup(GameObject collider)
    {
        collider.GetComponent<DashAbility>().enabled = true;
        base.Pickup(collider);
    }
}
