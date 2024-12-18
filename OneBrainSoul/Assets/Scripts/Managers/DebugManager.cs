using UnityEngine;

public class DebugManager : Singleton<DebugManager>
{
    [SerializeField] Vector3[] positions;
    private void OnGUI()
    {
        Event e = Event.current;
        if ((e.type == EventType.KeyDown && e.isKey && e.character == '\0'))
        {
            switch(e.keyCode)
            {
                case KeyCode.F1:
                case KeyCode.F2:
                case KeyCode.F3:
                case KeyCode.F4:
                case KeyCode.F5:
                case KeyCode.F6:
                case KeyCode.F7:
                case KeyCode.F8:
                case KeyCode.F9:
                case KeyCode.F10:
                case KeyCode.F11:
                case KeyCode.F12:
                    SetPlayersPosition((int)e.keyCode - (int)KeyCode.F1);
                    break;
                case KeyCode.M:
                    AudioManager.instance.musicVolume = 1f - AudioManager.instance.musicVolume;
                    break;
            }
        }
    }

    void SetPlayersPosition(int pos)
    {
        for (int i = 0; i < BraincellManager.Instance.playerControllers.Length && i < positions.Length; i++)
        {
            BraincellManager.Instance.playerControllers[i].transform.position = positions[pos];
        }
    }
}
