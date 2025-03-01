using UnityEngine;

namespace Menus.Main
{
    public class Title : MonoBehaviour
    {
        public void TitleAnimationFinished()
        {
            MainMenu.Instance.TitleAnimationFinished();
        }
    }
}