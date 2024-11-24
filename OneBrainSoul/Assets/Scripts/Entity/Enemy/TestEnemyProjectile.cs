using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemyProjectile : MonoBehaviour
{
    Vector3 direction;
    float speed;
    float lifeTime = 0f;
    public void Init(float lifeTime, Vector3 direction, float speed)
    {
        this.lifeTime = lifeTime;
        this.direction = direction;
        this.speed = speed;
        GetComponent<Rigidbody>().AddForce(direction * speed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        PlayerCharacterController player = collision.gameObject.GetComponent<PlayerCharacterController>();
        if (player != null)
        {
            player.health.Damage(1, gameObject);
        }
        Destroy(gameObject);
    }

    private void Update()
    {
        if (lifeTime <= 0f)
        {
            Destroy(gameObject);
        }
        lifeTime -= Time.deltaTime;
    }
}
