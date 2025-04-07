using Player;
using UnityEngine;

public class DeathFog : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerCharacterController player = other.GetComponent<PlayerCharacterController>();
        if (player == null) return;
        AudioManager.Instance.PlayOneShot(FMODEvents.instance.fog, transform.position);
        player.Respawn();
    }
}
