using Player;
using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    [SerializeField] Sprite[] faceSprites;
    [SerializeField] Sprite ghostSprite;
    [SerializeField] Image fill;
    [SerializeField] Image face;
    [SerializeField] PlayerCharacter player;

    [SerializeField][Range(0.2f,0.9f)] float smoothFactor = 0.6f;
    [SerializeField] Color damageTint = Color.red;
    [SerializeField] Color ghostTint = Color.magenta;

    float fillAmountTarget = 0f;

    void Start()
    {
        
    }
    void Update()
    {
        if (player.GetGhostTimeNormalized() < 0)
        {
            float hp = player.GetHealth();
            float maxHp = player.GetMaxHealth();
            fillAmountTarget = hp / maxHp;
            face.sprite = faceSprites[(int)(fillAmountTarget * (faceSprites.Length - 0.01f))];
        }
        else
        {
            fillAmountTarget = 1f - player.GetGhostTimeNormalized();
            face.sprite = ghostSprite;
        }
        float diff = fillAmountTarget - fill.fillAmount;
        bool negative = diff < 0f;
        float diffSmooth = Mathf.Pow(Mathf.Abs(diff) * (1f - smoothFactor), 1.5f);
        diffSmooth = negative ? -diffSmooth : diffSmooth;
        diff = Mathf.Abs(diff) < 0.03f ? diff : diffSmooth;
        fill.fillAmount = fill.fillAmount + diff;

        fill.color = player.GetGhostTimeNormalized() < 0 ? negative ? damageTint : Color.white : ghostTint;
    }
}
