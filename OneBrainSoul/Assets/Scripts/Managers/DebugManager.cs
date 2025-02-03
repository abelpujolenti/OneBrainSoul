using Player;
using UnityEngine;

namespace Managers
{
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
                        player.UnlockDash();
                        player.UnlockHook();
                        player.UnlockWallClimb();
                        player.SetJumpsAmount(2);
                        break;
                    case KeyCode.F2:
                        player.SetMoveSpeedMultiplier(5f);
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
}
