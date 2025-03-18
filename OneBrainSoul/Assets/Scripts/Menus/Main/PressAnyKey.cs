using UnityEngine;

namespace Menus.Main
{
    public class PressAnyKey : MonoBehaviour
    {
        [SerializeField] private Animator _animator;

        private void OnGUI()
        {
            if (!Input.anyKeyDown)
            {
                return;
            }
            
            _animator.SetBool("HasPressedAKey", true);
            AudioManager.instance.PlayOneShot(FMODEvents.instance.uiMainMenuPressAny, transform.position);
        }

        public void LabelDisappear()
        {
            MainMenu.Instance.ScrollDown();
        }
    }
}