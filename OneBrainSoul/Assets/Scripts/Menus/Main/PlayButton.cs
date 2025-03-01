using TMPro;
using UnityEngine;
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

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}