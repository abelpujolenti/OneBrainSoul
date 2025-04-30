using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] float openDuration = 1f;
    [SerializeField] Vector3 openVector;
    float openT = 0f;
    Vector3 startPos;
    bool open = false;

    private void Start()
    {
        startPos = transform.position;
    }

    public void Open()
    {
        open = true;
        openT = openDuration;
        AudioManager.Instance.PlayOneShot(FMODEvents.instance.secretDoor, transform.position);
    }

    private void FixedUpdate()
    {
        if (openT <= 0f) return;
        openT = Mathf.Max(0f, openT - Time.fixedDeltaTime);
        float t = openT / openDuration;

        transform.position = startPos + openVector * (1f - t);
    }
}
