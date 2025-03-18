using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Managers;

public class ElNomQueEtDonguiLaGana : MonoBehaviour
{
    private void OnTriggerEnter(Collider collider)
    {
        LoadSceneManager.Instance.LoadNextScene();
    }
}
