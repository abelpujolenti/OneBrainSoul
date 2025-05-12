using Player;
using UnityEngine;

namespace Managers
{
    public class DebugManager : Singleton<DebugManager>
    {
        PlayerCharacterController player;
        [SerializeField] Vector3[] positions;

        private void Start()
        {
            player = FindObjectOfType<PlayerCharacterController>();
        }

        private void OnGUI()
        {
            Event e = Event.current;
            if ((e.type == EventType.KeyDown && e.isKey && e.character == '\0'))
            {
                switch(e.keyCode)
                {
                    case KeyCode.F1:
                        player = FindObjectOfType<PlayerCharacterController>();
                        player.UnlockDash();
                        player.UnlockHook();
                        player.UnlockWallClimb();
                        player.SetJumpsAmount(2);
                        player.UnlockCharge();
                        break;
                    case KeyCode.F2:
                        player = FindObjectOfType<PlayerCharacterController>();
                        player.SetRespawn(new Vector3(-28, -9, 355));
                        player.Respawn();
                        break;
                    case KeyCode.F3:
                        player = FindObjectOfType<PlayerCharacterController>();
                        player.SetRespawn(new Vector3(-5, 23, 359));
                        player.Respawn();
                        break;
                    case KeyCode.F4:
                        player = FindObjectOfType<PlayerCharacterController>();
                        player.SetRespawn(new Vector3(-79, -39, 414));
                        player.Respawn();
                        break;
                    case KeyCode.F5:
                        player = FindObjectOfType<PlayerCharacterController>();
                        player.SetRespawn(new Vector3(-212, -15, 300));
                        player.Respawn();
                        break;
                    case KeyCode.M:
                        SettingsManager.Instance.SwitchMasterVolumeMute();
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
