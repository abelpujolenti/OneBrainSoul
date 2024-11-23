using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleTerrain : MonoBehaviour
{
    [SerializeField] Transform destroyParticle;

    public void Break(Vector3 pos)
    {
        var particle = Instantiate(destroyParticle).GetComponent<ParticleSystem>();
        particle.transform.position = pos;
        particle.Play();
        Destroy(gameObject);
    }
}
