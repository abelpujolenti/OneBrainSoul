using FMODUnity;
using UnityEngine;

public class FMODEvents : MonoBehaviour
{
    [field: Header("Snapshots")]

    [field: Header("Music")]
    [field: SerializeField] public EventReference music { get; private set; }

    [field: Header("SFX")]
    [field: SerializeField] public EventReference heal { get; private set; }
    [field: SerializeField] public EventReference healed { get; private set; }
    [field: SerializeField] public EventReference pickupItem { get; private set; }
    [field: SerializeField] public EventReference impact { get; private set; }
    [field: SerializeField] public EventReference shove { get; private set; }
    [field: SerializeField] public EventReference slam { get; private set; }
    [field: SerializeField] public EventReference teleport { get; private set; }
    [field: SerializeField] public EventReference teleportOut { get; private set; }
    [field: SerializeField] public EventReference throwRock { get; private set; }
    [field: SerializeField] public EventReference tornado { get; private set; }
    [field: SerializeField] public EventReference uiExit { get; private set; }
    [field: SerializeField] public EventReference uiGameStart { get; private set; }
    [field: SerializeField] public EventReference uiMainMenuPressAny { get; private set; }
    [field: SerializeField] public EventReference uiSelect { get; private set; }
    [field: SerializeField] public EventReference fog { get; private set; }
    [field: SerializeField] public EventReference hookThrow { get; private set; }
    [field: SerializeField] public EventReference charge { get; private set; }
    [field: SerializeField] public EventReference dash { get; private set; }
    [field: SerializeField] public EventReference enemyAttack { get; private set; }
    [field: SerializeField] public EventReference enemyDamage { get; private set; }
    [field: SerializeField] public EventReference whipAttack { get; private set; }
    [field: SerializeField] public EventReference hammerAttack { get; private set; }
    [field: SerializeField] public EventReference wandAttack { get; private set; }
    [field: SerializeField] public EventReference openSwitchMode { get; private set; }
    [field: SerializeField] public EventReference swap { get; private set; }
    [field: SerializeField] public EventReference catDamage { get; private set; }
    [field: SerializeField] public EventReference land { get; private set; }
    [field: SerializeField] public EventReference playerFootsteps { get; private set; }
    [field: SerializeField] public EventReference trifaceFootsteps { get; private set; }
    [field: SerializeField] public EventReference longArmsTurn { get; private set; }

    public static FMODEvents instance { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
}
