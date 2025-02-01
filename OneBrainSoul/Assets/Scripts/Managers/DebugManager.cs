using Player.Abilities;
using UnityEngine;

public class DebugManager : Singleton<DebugManager>
{
    [SerializeField] PlayerCharacterController player;
    [SerializeField] Vector3[] positions;
    private void OnGUI()
    {
        Event e = Event.current;
        if ((e.type == EventType.KeyDown && e.isKey && e.character == '\0'))
        {
            switch(e.keyCode)
            {
                case KeyCode.F1:
                    player.GetComponent<DashAbility>().enabled = true;
                    player.GetComponent<HookAbility>().enabled = true;
                    player.GetComponent<WallClimbAbility>().enabled = true;
                    player.jumps = 2;
                    break;
                case KeyCode.F2:
                    player.moveSpeedMultiplier = 5f;
                    break;
            }
        }
    }

    void SetPlayersPosition(int pos)
    {
        player.enabled = false;
        player.transform.position = positions[pos];
        player.enabled = true;
    }
}
