using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

public class DeathFog : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerCharacterController player = other.GetComponent<PlayerCharacterController>();
        if (player == null) return;
        player.Respawn();
    }
}
