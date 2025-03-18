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
            
            LoadSceneManager.Instance.ExitGame();
        }
    }
}