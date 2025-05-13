using FMODUnity;
using UnityEngine;

public class FMODEvents : MonoBehaviour
{
    [field: Header("Snapshots")]

    [field: Header("Music")]
    [field: SerializeField] public EventReference music { get; private set; }
    [field: Header("Ambient")]
    [field: SerializeField] public EventReference ambient { get; private set; }

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
    [field: SerializeField] public EventReference hookThrow { get; private set; }
    [field: SerializeField] public EventReference charge { get; private set; }
    [field: SerializeField] public EventReference dash { get; private set; }
    [field: SerializeField] public EventReference doubleJump { get; private set; }
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
    [field: SerializeField] public EventReference brasero { get; private set; }
    [field: SerializeField] public EventReference catIdle { get; private set; }
    [field: SerializeField] public EventReference combatDoor_Ambient { get; private set; }
    [field: SerializeField] public EventReference combatDoor_Closed { get; private set; }
    [field: SerializeField] public EventReference combatDoor_Open { get; private set; }
    [field: SerializeField] public EventReference combatDoor_Through { get; private set; }
    [field: SerializeField] public EventReference enemyAlert { get; private set; }
    [field: SerializeField] public EventReference enemyDeath { get; private set; }
    [field: SerializeField] public EventReference enemySpawn { get; private set; }
    [field: SerializeField] public EventReference floatingLanterns { get; private set; }
    [field: SerializeField] public EventReference ghostMode { get; private set; }
    [field: SerializeField] public EventReference lowHealth { get; private set; }
    [field: SerializeField] public EventReference slide { get; private set; }
    [field: SerializeField] public EventReference thunder { get; private set; }
    [field: SerializeField] public EventReference toxicFog { get; private set; }
    [field: SerializeField] public EventReference secretDoor { get; private set; }
    [field: SerializeField] public EventReference longArmsSmashAttack { get; private set; }

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
