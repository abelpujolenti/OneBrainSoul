using Player;
using UnityEngine;
using ECS.Entities.AI;

public class DeathFog : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerCharacterController player = other.GetComponent<PlayerCharacterController>();
        if (player != null)
        {
            AudioManager.Instance.PlayOneShot(FMODEvents.instance.toxicFog, transform.position);
            player.Respawn();
            return;
        }
        AgentEntity agentEntity = other.GetComponent<AgentEntity>();
        if (agentEntity != null)
        {
            Destroy(other.gameObject);
        }
    }
}
