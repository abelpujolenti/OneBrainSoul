using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;

public class SwitchModeUI : MonoBehaviour
{
    [GradientUsage(true)]
    public Gradient arrowGradient;
    public float triggerRadius = 150f;

    Image wheel;
    public Image cursor;
    public Image[] allyIcons;
    Vector2 mousePosition;
    float wheelRadius;
    float allyIconRadius;
    int selectedAlly = -1;
    int selectedIcon = -1;

    private void OnEnable()
    {
        wheel = GetComponent<Image>();
        wheelRadius = wheel.rectTransform.sizeDelta.y * 2f / 7f;
        allyIconRadius = allyIcons[0].rectTransform.sizeDelta.x * .5f;
    }

    public void ReleaseSwitch(PlayerCharacterController player)
    {
        if (selectedAlly >= 0)
        {
            player.braincellManager.SwitchCommand(selectedAlly);
        }
    }

    public void SwitchModeUpdate(PlayerCharacterController player)
    {

        if (player.switchModeTime == 0)
        {
            mousePosition = Vector2.zero;
        }

        float l = mousePosition.magnitude;
        if (l > wheelRadius)
        {
            mousePosition = mousePosition.normalized * wheelRadius;
        }
        cursor.transform.localPosition = mousePosition;
        cursor.transform.rotation = Quaternion.FromToRotation(Vector2.down, mousePosition.normalized);
        cursor.color = arrowGradient.Evaluate(Mathf.Clamp01(l / wheelRadius));
        cursor.transform.localScale = Vector3.one * (1f + .8f * l / wheelRadius);

        float minDist = Mathf.Infinity;
        int i = 0;
        int p = 0;
        selectedAlly = -1;
        selectedIcon = -1;
        foreach (var ally in player.braincellManager.playerControllers)
        {
            if (ally == player)
            {
                p++;
                continue;
            }
            Quaternion r = Quaternion.Inverse(player.orientation.rotation);
            Vector3 relativePos = r * (ally.transform.position - player.transform.position);
            Vector2 iconPos = new Vector2(relativePos.x, relativePos.z).normalized * (wheelRadius + allyIconRadius);
            allyIcons[i].transform.localPosition = iconPos;
            allyIcons[i].color = player.braincellManager.allyIconGradient.Evaluate(p / (float)allyIcons.Length);
            allyIcons[i].transform.localScale = Vector3.one;
            float dist = Vector2.Distance(mousePosition, iconPos);
            if (dist < triggerRadius && dist <= minDist)
            {
                minDist = dist;
                selectedIcon = i;
                selectedAlly = p;
            }
            i++;
            p++;
        }

        if (selectedAlly >= 0)
        {
            allyIcons[selectedIcon].transform.localScale = Vector3.one * Mathf.Pow(Mathf.Max(1f, 3f * l / (wheelRadius + allyIconRadius)), .8f);
        }
        mousePosition += new Vector2(Input.GetAxis("Mouse X") * player.cam.horizontalSensitivity, Input.GetAxis("Mouse Y") * player.cam.verticalSensitivity) * 4f * Time.unscaledDeltaTime;
    }
}
