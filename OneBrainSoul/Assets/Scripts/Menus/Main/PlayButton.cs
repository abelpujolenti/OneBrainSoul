using Managers;
using UnityEngine.SceneManagement;

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

            GameManager.Instance.LoadSceneIndex(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}