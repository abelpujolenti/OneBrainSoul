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
            AudioManager.instance.PlayOneShot(FMODEvents.instance.uiExit, transform.position);
            LoadSceneManager.Instance.ExitGame();
        }
    }
}