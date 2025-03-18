using UnityEngine;

public class Billboard : MonoBehaviour
{
    private void LateUpdate()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;
        transform.localRotation = mainCamera.transform.localRotation;
    }
}
