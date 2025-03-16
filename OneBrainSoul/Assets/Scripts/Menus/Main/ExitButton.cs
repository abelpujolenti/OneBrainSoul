using Managers;

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
            
            GameManager.Instance.ExitGame();
        }
    }
}