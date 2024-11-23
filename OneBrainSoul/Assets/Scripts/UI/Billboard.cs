using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class Billboard : MonoBehaviour
{
    [SerializeField] private float distanceScaling = 0f;
    private void LateUpdate()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;
        transform.rotation = mainCamera.transform.rotation;
        transform.localScale = Vector3.one * (1f + Vector3.Distance(transform.position, mainCamera.transform.position) * distanceScaling);
    }
}
