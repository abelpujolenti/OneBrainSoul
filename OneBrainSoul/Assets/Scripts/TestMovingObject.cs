using UnityEngine;

public class TestMovingObject : MonoBehaviour
{
    Vector3 dir = Vector3.zero;
    float speed = 1f;
    // Start is called before the first frame update
    void Start()
    {
        dir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        speed = Random.Range(10f, 50f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += dir * Time.deltaTime * speed * Mathf.Sin(Time.time * .6f);
    }
}
