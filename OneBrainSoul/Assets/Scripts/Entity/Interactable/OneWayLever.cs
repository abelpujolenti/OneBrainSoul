using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneWayLever : Item
{
    [SerializeField] Transform door;
    [SerializeField] float openDuration = 1f;
    float openT = 0f;
    Vector3 startPos;
    bool open = false;

    private void Start()
    {
        startPos = door.position;
    }

    protected override void TryPickup(GameObject collider)
    {
    }

    protected override void Pickup(GameObject collider)
    {
        open = true;
        openT = openDuration;
        base.Pickup(collider);
    }

    private void FixedUpdate()
    {
        if (!open) return;
        openT -= Mathf.Max(0f, openT - Time.fixedDeltaTime);
        door.transform.position = startPos + Vector3.up * 50 * (openT / openDuration);
        if (openT <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
