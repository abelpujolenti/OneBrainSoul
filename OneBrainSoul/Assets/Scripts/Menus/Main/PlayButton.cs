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

            LoadSceneManager.Instance.LoadNextScene();
        }
    }
}