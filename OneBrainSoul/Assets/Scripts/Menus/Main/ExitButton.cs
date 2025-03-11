using TMPro;
using UnityEngine;

namespace Menus.Main
{
    public class ExitButton : MainMenuButton
    {
        public override void Press()
        {
            if (!_canPress)
            {
                return;
            }
            
            Application.Quit();
        }
    }
}