using UnityEngine;

namespace Player.Camera
{
    public class MatchCameraFOV : MonoBehaviour
    {
        UnityEngine.Camera cam;
        [SerializeField] UnityEngine.Camera camToMatch;
        void Start()
        {
            cam = GetComponent<UnityEngine.Camera>();
        }

        void Update()
        {
            cam.fieldOfView = camToMatch.fieldOfView;
        }
    }
}
