using Managers;

namespace Menus.Main
{
    public class PlayButton : MainMenuButton
    {
        public override void Press()
        {
            if (!_canPress)
            {
                return;
            }

            AudioManager.instance.PlayOneShot(FMODEvents.instance.uiGameStart, transform.position);
            LoadSceneManager.Instance.LoadNextScene();
        }
    }
}