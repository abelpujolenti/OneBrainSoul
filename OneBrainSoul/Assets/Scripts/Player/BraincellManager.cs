using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BraincellManager : MonoBehaviour
{
    [SerializeField] PlayerCharacterController[] playerControllers;
    [SerializeField] float transitionDuration = 0.2f;
    [SerializeField] AnimationCurve slowDownCurve;
    [SerializeField] AnimationCurve lensDistortionCurve;
    [SerializeField] VolumeProfile volumeProfile;
    [SerializeField] URPManager urpManager;

    int currentCharacter = 0;
    float transitionTime = 0f;

    private void OnGUI()
    {
        Event e = Event.current;
        if ((e.type == EventType.KeyDown && e.isKey && e.character == '\0'))
        {
            switch (e.keyCode)
            {
                case KeyCode.Alpha1:
                case KeyCode.Quote:
                    SwitchCommand(0);
                    break;
                case KeyCode.Alpha2:
                case KeyCode.Slash:
                    SwitchCommand(1);
                    break;
                case KeyCode.Alpha3:
                case KeyCode.Equals:
                    SwitchCommand(2);
                    break;
                default:
                    break;
            }
        }
        else if (e.isScrollWheel)
        {
            int c = (currentCharacter - (int)Input.mouseScrollDelta.y + playerControllers.Length) % playerControllers.Length;
            SwitchCommand(c);
        }
    }

    private void Update()
    {
        if (transitionTime > 0f)
        {
            TransitionUpdate();
        }
        transitionTime = Mathf.Max(0f, transitionTime - Time.deltaTime);
    }

    private void TransitionUpdate()
    {
        float t = transitionTime / transitionDuration;
        float c = slowDownCurve.Evaluate(1 - t);
        Time.timeScale = 1 - c;
        if (transitionTime - Time.deltaTime <= 0f)
        {
            Time.timeScale = 1f;
        }
        LensDistortion lensDistortion;
        foreach (VolumeComponent vc in volumeProfile.components)
        {
            if (vc.GetType() == typeof(LensDistortion))
            {
                lensDistortion = vc as LensDistortion;
                lensDistortion.intensity.value = lensDistortionCurve.Evaluate(1 - t);
                break;
            }
        }
    }

    private void SwitchCommand(int id)
    {
        if (id < 0 || id >= playerControllers.Length) return;
        if (transitionTime > 0) return;
        if (id == currentCharacter) return;
        Switch(id);
    }

    private void Switch(int id)
    {
        currentCharacter = id;
        transitionTime = transitionDuration;
        urpManager.BraincellSwitchTransition(transitionDuration * 1.3f);

        for (int i = 0; i < playerControllers.Length; i++)
        {
            if (i == id)
            {
                ActivatePlayerController(playerControllers[i]);
                continue;
            }
            DeactivatePlayerController(playerControllers[i]);
        }
    }

    private void ActivatePlayerController(PlayerCharacterController c)
    {
        c.cam.gameObject.SetActive(true);
        c.display.SetActive(false);
        c.braincell = true;
    } 
    private void DeactivatePlayerController(PlayerCharacterController c)
    {
        c.cam.gameObject.SetActive(false);
        c.display.SetActive(true);
        c.braincell = false;
    } 
}
