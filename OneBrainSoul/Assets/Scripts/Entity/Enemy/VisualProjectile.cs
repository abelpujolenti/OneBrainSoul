using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class VisualProjectile : MonoBehaviour
{
    Rigidbody rb;
    Transform source;
    Transform target;
    Action<GameObject> callback;
    float speed;
    float drag = 8f;
    float lifeTime = 0f;
    public void Init(Transform source, Transform target, float speed, Action<GameObject> callback)
    {
        this.source = source;
        this.target = target;
        this.speed = speed;
        this.callback = callback;
        rb = GetComponent<Rigidbody>();
        rb.AddForce((target.position - source.position).normalized * speed * 3f, ForceMode.Acceleration);
    }

    void Update()
    {
        if (target == null || source == null || lifeTime > 10f)
        {
            Destroy(gameObject);
            return;
        }
        if (Vector3.Distance(target.position, transform.position) < 3f)
        {
            callback.Invoke(target.gameObject);   
            Destroy(gameObject);
            return;
        }

        lifeTime += Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (target == null || source == null)
        {
            Destroy(gameObject);
            return;
        }
        rb.AddForce((target.position - source.position).normalized * speed, ForceMode.Acceleration);
        rb.AddForce(-(rb.velocity.normalized) * drag, ForceMode.Acceleration);
    }
}
