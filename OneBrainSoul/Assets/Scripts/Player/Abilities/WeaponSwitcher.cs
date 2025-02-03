using Combat;
using UnityEngine;

namespace Player.Abilities
{
    public class WeaponSwitcher : MonoBehaviour
    {
        [SerializeField] private Weapon[] _weapons;
        [SerializeField] private float _transitionDuration = 0.1f;
        private int _currentWeapon = 0;
        private float _transitionTime = 0f;

        public void Setup(Weapon[] weapons, float transitionDuration)
        {
            _weapons = weapons;
            _transitionDuration = transitionDuration;
        }

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
                int c = (_currentWeapon - (int)Input.mouseScrollDelta.y + _weapons.Length) % _weapons.Length;
                SwitchCommand(c);
            }
        }

        private void Update()
        {
            if (_transitionTime > 0f)
            {
                TransitionUpdate();
            }
            _transitionTime = Mathf.Max(0f, _transitionTime - Time.deltaTime);
        }

        private void TransitionUpdate()
        {

        }

        public void SwitchCommand(int id)
        {
            if (id < 0 || id >= _weapons.Length) return;
            if (_transitionTime > 0) return;
            if (id == _currentWeapon) return;
            Switch(id);
        }

        private void Switch(int id)
        {
            _transitionTime = _transitionDuration;
            PostProcessingManager.Instance.BraincellSwitchTransition(_transitionDuration * 1.3f);

            _weapons[_currentWeapon].Deactivate();
            _weapons[id].Activate();

            _currentWeapon = id;

            AudioManager.instance.PlayOneShot(FMODEvents.instance.swap, transform.position);
        }
    }
}
