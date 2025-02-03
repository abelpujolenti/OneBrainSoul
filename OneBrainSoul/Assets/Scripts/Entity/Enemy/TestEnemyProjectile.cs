using Player;
using UnityEngine;

public class TestEnemyProjectile : MonoBehaviour
{
    Vector3 direction;
    float speed;
    int damage;
    float lifeTime = 0f;
    public void Init(float lifeTime, Vector3 direction, float speed, int damage)
    {
        this.lifeTime = lifeTime;
        this.direction = direction;
        this.speed = speed;
        this.damage = damage;
        GetComponent<Rigidbody>().AddForce(direction * speed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        PlayerCharacterController player = collision.gameObject.GetComponent<PlayerCharacterController>();
        if (player != null)
        {
            //player.health.Damage(damage, gameObject);
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
