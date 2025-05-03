using Managers;
using UnityEngine;

public class ElNomQueEtDonguiLaGana : MonoBehaviour
{
    private void OnTriggerEnter(Collider collider)
    {
        LoadSceneManager.Instance.LoadNextScene();
    }
}
