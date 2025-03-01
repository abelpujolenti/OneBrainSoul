using System;
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
        }

        public void LabelDisappear()
        {
            MainMenu.Instance.ScrollDown();
        }
    }
}