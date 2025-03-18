using Player;
using UnityEngine;

public class CombatRoomTrigger : MonoBehaviour
{
    CombatRoom combatRoom;

    private void Start()
    {
        combatRoom = GetComponentInParent<CombatRoom>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.gameObject.GetComponent<PlayerCharacterController>();
        if (player != null)
        {
            combatRoom.Enter(player);
        }
    }
}
