using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

public class RisingFog : MonoBehaviour
{
    private Vector3 startPos;
    public bool triggered;
    [SerializeField] private float height = 90f;
    [SerializeField] private float speed = 10f;
    
    void Start()
    {
        startPos = transform.position;
    }

    public void BeginRising()
    {
        if (triggered) return;
        triggered = true;
    }

    public void ResetPosition()
    {
        triggered = false;
        transform.position = startPos;
    }

    void FixedUpdate()
    {
        if (!triggered || transform.position.y >= startPos.y + height) return;

        transform.position += Vector3.up * speed * Time.fixedDeltaTime;
    }
}
