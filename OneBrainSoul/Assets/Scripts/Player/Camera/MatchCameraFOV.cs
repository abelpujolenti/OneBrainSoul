using UnityEngine;

public class MatchCameraFOV : MonoBehaviour
{
    Camera cam;
    [SerializeField] Camera camToMatch;
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        cam.fieldOfView = camToMatch.fieldOfView;
    }
}
