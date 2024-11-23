using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActiveDamageTakingEntityManager : MonoBehaviour
{
    public List<DamageTakingEntity> damageTakingEntities;

    private void Start()
    {
        //Find better way to do this
        damageTakingEntities = FindObjectsByType<DamageTakingEntity>(FindObjectsSortMode.None).ToList();
    }

    private void LateUpdate()
    {
        for (int i = 0; i < damageTakingEntities.Count; i++)
        {
            if (damageTakingEntities[i] == null)
            {
                damageTakingEntities.Remove(damageTakingEntities[i]);
                i--;
            }
        }
    }
}
