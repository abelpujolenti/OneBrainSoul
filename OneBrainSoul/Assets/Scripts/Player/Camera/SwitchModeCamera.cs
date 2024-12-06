using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchModeCamera : MonoBehaviour
{
    [Header("Sensitivity")]
    public float verticalSensitivity = 400f;
    public float horizontalSensitivity = 400f;

    [Header("Orientation")]
    public Transform orientation;

    public PlayerCharacterController player;

    private float xRotation = 0f;
    private float yRotation = 0f;

    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        xRotation = orientation.rotation.eulerAngles.x;
        yRotation = orientation.rotation.eulerAngles.y;
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }

    void Update()
    {
        // Uses unscaled delta time so it's not affected by slow motion mechanics
        float mouseX = Input.GetAxis("Mouse X") * horizontalSensitivity * Time.unscaledDeltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * verticalSensitivity * Time.unscaledDeltaTime;


        float t = Mathf.Pow(player.switchModeTime, 1f / player.switchModeFalloffPower);
        float rx = Mathf.Pow(t, 0.5f) * player.switchModeRotationFactor;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -75f + rx, 75f - rx);

        // Rotate camera
        transform.localRotation = Quaternion.Euler(rx + xRotation, yRotation, 0f);

    }
}
