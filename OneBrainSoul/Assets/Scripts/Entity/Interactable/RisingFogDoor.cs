using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RisingFogDoor : MonoBehaviour
{
    [SerializeField] RisingFog risingFog;
    PlayerCharacter player;

    private void OnTriggerEnter(Collider other)
    {
        player = other.gameObject.GetComponent<PlayerCharacter>();
        if (player != null)
        {
            risingFog.BeginRising();
        }
    }

    private void Update()
    {
        if (risingFog.triggered && player.GetGhostTimeNormalized() >= 0f)
        {
            risingFog.ResetPosition();
        }
    }
}
