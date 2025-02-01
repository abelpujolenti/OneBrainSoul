using UnityEngine;

namespace Player.Abilities
{
    public class WeaponSwitcher : MonoBehaviour
    {
        [SerializeField] Weapon[] weapons;
        [SerializeField] float transitionDuration = 0.1f;
        int currentWeapon = 0;
        float transitionTime = 0f;
        private void OnGUI()
        {
            Event e = Event.current;
            if ((e.type == EventType.KeyDown && e.isKey && e.character == '\0'))
            {
                switch (e.keyCode)
                {
                    case KeyCode.Alpha1:
                    case KeyCode.Quote:
                        SwitchCommand(0);
                        break;
                    case KeyCode.Alpha2:
                    case KeyCode.Slash:
                        SwitchCommand(1);
                        break;
                    case KeyCode.Alpha3:
                    case KeyCode.Equals:
                        SwitchCommand(2);
                        break;
                    default:
                        break;
                }
            }
            else if (e.isScrollWheel)
            {
                int c = (currentWeapon - (int)Input.mouseScrollDelta.y + weapons.Length) % weapons.Length;
                SwitchCommand(c);
            }
        }

        private void Update()
        {
            if (transitionTime > 0f)
            {
                TransitionUpdate();
            }
            transitionTime = Mathf.Max(0f, transitionTime - Time.deltaTime);
        }

        private void TransitionUpdate()
        {

        }

        public void SwitchCommand(int id)
        {
            if (id < 0 || id >= weapons.Length) return;
            if (transitionTime > 0) return;
            if (id == currentWeapon) return;
            Switch(id);
        }

        private void Switch(int id)
        {
            transitionTime = transitionDuration;
            PostProcessingManager.Instance.BraincellSwitchTransition(transitionDuration * 1.3f);

            weapons[currentWeapon].Deactivate();
            weapons[id].Activate();

            currentWeapon = id;

            AudioManager.instance.PlayOneShot(FMODEvents.instance.swap, transform.position);
        }
    }
}
