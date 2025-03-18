using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using Button = UnityEngine.UI.Button;

namespace Menus.Main
{
    public abstract class MainMenuButton : MyButton
    {
        protected bool _canPress;

        [SerializeField] private Button _button;
        
        public void EnablePress()
        {
            _button.enabled = true;
            _canPress = true;
        }

        public abstract void Press();
    }
}