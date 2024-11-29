using System.Collections.Generic;
using UnityEngine;

public class ActiveEnemyManager : MonoBehaviour
{
    public List<EnemyTest> activeEnemies;

    private void Update()
    {
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            if (activeEnemies[i] == null)
            {
                activeEnemies.Remove(activeEnemies[i]);
                i--;
            }
        }
    }
}
