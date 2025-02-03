using UnityEngine;

namespace Player.Effects
{
    public class Hitstop : MonoBehaviour
    {
        float hitstopT = 0f;
        float aftershockT = 0f;

        //TODO ADRI NO HO TOCO PERQUÈ TENS UN ELSE IF, TU SAPS PERQUÈ ES AIXÓ
        //PERÒ AIXÓ HAURIA D'ESTAR EN UNA COROUTINE, NO S'HA D'EXECUTAR TOTA L'ESTONA
        void Update()
        {
            if (hitstopT > 0)
            {
                Time.timeScale = 0.02f;
                if (hitstopT - Time.unscaledDeltaTime <= 0f)
                {
                    Time.timeScale = 1f;
                }
            }
            else if (aftershockT > 0)
            {
                Time.timeScale = 0.2f;
                if (aftershockT - Time.unscaledDeltaTime <= 0f)
                {
                    Time.timeScale = 1f;
                }
            }
            hitstopT = Mathf.Max(0f, hitstopT - Time.unscaledDeltaTime);
            aftershockT = Mathf.Max(0f, aftershockT - Time.unscaledDeltaTime);
        }

        public void Add(float t)
        {
            hitstopT += t;
        }

        public void AddAftershock(float t)
        {
            aftershockT += t;
        }
    }
}
