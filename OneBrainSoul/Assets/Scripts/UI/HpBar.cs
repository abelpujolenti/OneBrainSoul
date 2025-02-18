using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    [SerializeField] Sprite[] faceSprites;
    [SerializeField] Image fill;
    [SerializeField] Image face;
    [SerializeField] Player.PlayerCharacter player;
    void Start()
    {
        
    }
    void Update()
    {
        float hp = player.GetHealth();
        float maxHp = player.GetMaxHealth();
        float p = hp / maxHp;
        fill.fillAmount = p;
        face.sprite = faceSprites[(int)(p * (faceSprites.Length - 1))];
    }
}
