using UnityEngine;

namespace Menus.Main
{
    public class ThunderSound : MonoBehaviour
    {
        public void PlayThunderSound()
        {
            AudioManager.Instance.PlayOneShot(FMODEvents.instance.thunder, transform.position);
        }
    }
}