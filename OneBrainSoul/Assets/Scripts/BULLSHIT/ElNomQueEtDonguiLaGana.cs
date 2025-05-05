using Managers;
using UnityEngine;
using Player;

public class ElNomQueEtDonguiLaGana : MonoBehaviour
{
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.GetComponent<PlayerCharacterController>() != null) {
            LoadSceneManager.Instance.LoadNextScene();
        }

    }
}
