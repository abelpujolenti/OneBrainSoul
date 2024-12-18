using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour
{
    [field: Header("Snapshots")]
    
    [field: Header("Hook SFX")]
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
