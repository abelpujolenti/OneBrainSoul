using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HookUI : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] Transform icon;
    [SerializeField] private Vector3 rootPosition;
    [SerializeField] private float width = 55f;
    [SerializeField] private float angleDistance = 40f;

    [ColorUsage(true,true)][SerializeField] Color iconColor;
    [ColorUsage(true,true)][SerializeField] Color iconColorOuter;
    [ColorUsage(true,true)][SerializeField] Color iconColorOff;
    [ColorUsage(true,true)][SerializeField] Color iconColorOuterOff;

    List<Image> icons = new List<Image>();

    int maxCharges;

    public void SetMaxCharges(int maxCharges)
    {
        for (int i = 0; i < icons.Count; i++)
        {
            Destroy(icons[i].gameObject);
        }

        this.maxCharges = maxCharges;
        canvas = GetComponent<Canvas>();

        for (int i = maxCharges - 1; i >= 0; i--)
        {
            Image currIcon = Instantiate(icon, transform).GetComponent<Image>();
            currIcon.transform.localRotation = Quaternion.identity;
            currIcon.transform.localScale = Vector3.one;

            float a = (180f + angleDistance * (i - maxCharges / 2f + 0.5f)) * Mathf.Deg2Rad;
            
            currIcon.transform.localPosition = new Vector3(Mathf.Sin(a) * width, Mathf.Cos(a) * width, 0f) + rootPosition;
            icons.Add(currIcon);
        }
    }

    public void UpdateUI(int amount, float rechargeTime, float rechargeDuration)
    {
        for (int i = 0; i < icons.Count; i++)
        {
            int i2 = (i % 2) * (icons.Count - 1) + (int)Mathf.Pow(-1, i) * i / 2;
            icons[i2].color = i < icons.Count - amount ? iconColorOff : iconColor;
            //icons[i2].transform.GetChild(0).GetComponent<Image>().color = i < icons.Count - amount ? iconColorOuterOff : iconColorOuter;
            //icons[i2].transform.GetChild(1).GetComponent<Image>().color = i < icons.Count - amount ? iconColorOuterOff : iconColorOuter;
            if (i == icons.Count - amount - 1)
            {
                icons[i2].transform.GetChild(0).GetComponent<Image>().fillAmount = rechargeTime / rechargeDuration;
                icons[i2].transform.GetChild(1).GetComponent<Image>().fillAmount = rechargeTime / rechargeDuration;
                icons[i2].transform.GetChild(2).GetComponent<Image>().fillAmount = 1f - rechargeTime / rechargeDuration;
                icons[i2].transform.GetChild(3).GetComponent<Image>().fillAmount = 1f - rechargeTime / rechargeDuration;
            }
            else if (i < icons.Count - amount - 1)
            {
                icons[i2].transform.GetChild(0).GetComponent<Image>().fillAmount = 0f;
                icons[i2].transform.GetChild(1).GetComponent<Image>().fillAmount = 0f;
                icons[i2].transform.GetChild(2).GetComponent<Image>().fillAmount = 1f;
                icons[i2].transform.GetChild(3).GetComponent<Image>().fillAmount = 1f;
            }
            else
            {
                icons[i2].transform.GetChild(0).GetComponent<Image>().fillAmount = 1f;
                icons[i2].transform.GetChild(1).GetComponent<Image>().fillAmount = 1f;
                icons[i2].transform.GetChild(2).GetComponent<Image>().fillAmount = 0f;
                icons[i2].transform.GetChild(3).GetComponent<Image>().fillAmount = 0f;
            }
        }
    }
}
